using Infrastructure;
using LearningExtraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;
using System.Threading;
using DataClasses;
using Config;
using Sound;
using System.IO;
using System.Collections.Concurrent;
using Microsoft.EntityFrameworkCore;

namespace Linguine
{

    
    public partial class MainModel
    {

        private ICanAnalyseText?      TextAnalyser     { get; set; }

        private ICanParseDefinitions? DefinitionParser { get; set; }

        private ICanPronounce?        Pronouncer       { get; set; }

        private int CharsToProcess { get; set; } = 1_000;

        private Tuple<String?, List<String>?, bool, int> GetNextChunkInfo(TextualMedia tm)
        {
            int end = StatementManager.IndexOffEndOfLastStatement(tm);

            if (end == tm.Text.Length)
            {
                return Tuple.Create<String?, List<String>?, bool, int>(null, null, false, -1);
            }
            else if (end == -1 /* indicates no statements exist for this TextualMedia */)
            {
                end = 0; // ensure no weird stuff
            }

            int firstChar = end;

            List<String> previousContext = GetPreviousContext(tm);

            int charSpan = CharsToProcess;

            LanguageCode target = ConfigManager.Config.Languages.TargetLanguage;
            charSpan = (int)(charSpan * LanguageCodeDetails.Density(target));

            bool isTail = false;

            if (tm.Text.Length - firstChar < charSpan)
            {
                charSpan = tm.Text.Length - firstChar;
                isTail = true; // TODO - should this be <=??
            }

            String chunk = tm.Text.Substring(firstChar, charSpan);

            return Tuple.Create<String?, List<String>?, bool, int>(chunk, previousContext, isTail, firstChar);
        }

        // TODO - make use of the cancellation token?
        internal async Task ProcessNextChunkForSession(int sessionID)
        {
            TextualMedia? tm = GetSessionFromID(sessionID)?.TextualMedia ?? null;
            if (tm is null) { return; }

            await ProcessNextChunk(tm, new CancellationToken());
        }

        internal async Task<int> ProcessNextChunk(TextualMedia tm, CancellationToken token)
        {

            // determine the chunk of text to process next
            (String? text, List<String>? context, bool isTail, int firstChar) = GetNextChunkInfo(tm);
            if (text is null || context is null) { return -1; }

            List<ProtoStatement>? builders = await DoProcessingStep(text, context, isTail, token);

            if (token.IsCancellationRequested)
            {
                Log.Information("cancelled processing step before forming proto statements");
                return 0;
            }

            if (builders is null)
            {
                return 0;
            }

            List<Statement> statements = FromProtoStatements(builders, tm, firstChar);

            StatementManager.AddStatements(statements);

            if (token.IsCancellationRequested)
            {
                Log.Warning("processing step added statements, but didn't parse definitions!, cancellation requested too early");
                   
                // throw new NotImplementedException("can't resolve this yet!");

                // lets just carry on, since throwing is a bit much
            }

            var tasks = new List<Task>();

            if (ConfigManager.Config.LearningForeignLanguage())
            {
                tasks.Add(ParseDefinitions(statements));
            }

            tasks.Add(PronounceDefinitions(statements));

            tasks.Add(VocaliseDefinitions(statements));

            await Task.WhenAll(tasks);

            return statements.Last().LastCharIndex;
        }

        private async Task VocaliseDefinitions(List<Statement> statements)
        {
            HashSet<DictionaryDefinition> definitions = StatementManager.GetAllUniqueDefinitions(statements);

            using var context = _linguineDbContextFactory.CreateDbContext();

            List<DictionaryDefinition> newDefinitions = definitions.Where(
                d => DefinitionVocalisationManager.HasAnyFiles(d, context) == false).ToList();

            context.Dispose();

            if (newDefinitions.Count == 0)
            {
                return;
            }

            var tasks = new List<Task>();

            LanguageCode target = ConfigManager.Config.Languages.TargetLanguage;

            Voice voice = Voice.Fenrir; // TODO - cycle these?

            var recordsToSave = new ConcurrentBag<VocalisedDefinitionFile>();

            foreach (var def in newDefinitions)
            {
                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        string fileName = Path.Combine(target.ToString(),
                                                       "Definitions",
                                                       voice.ToString(),
                                                       def.GetSafeFileName() + ".wav");

                        byte[] audio = await Talker.TextToSpeech(def.Word, voice, target);

                        var record = await DefinitionVocalisationManager.AddVocalisationAsync(audio, def, voice, fileName);
                        recordsToSave.Add(record);
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"Failed to vocalise definition {def.ID}: {ex.Message}");
                    }
                }));
            }

            await Task.WhenAll(tasks);

            using var contextTmp = _linguineDbContextFactory.CreateDbContext();

            foreach (VocalisedDefinitionFile record in recordsToSave)
            {
                await contextTmp.AddAsync(record);
                await contextTmp.SaveChangesAsync();

                contextTmp.ChangeTracker.Clear();
            }
        }

        private async Task PronounceDefinitions(List<Statement> statements)
        {
            if (Pronouncer is null)
            {
                StartPronunciationEngine();
            }

            if (Pronouncer is null)
            {
                Log.Fatal("Couldn't start pronunciation engine!");
                throw new Exception("failed to start pronunciation engine");
            }

            HashSet<DictionaryDefinition> definitions = StatementManager.GetAllUniqueDefinitions(statements);


            List<DictionaryDefinition> newDefinitionsList = definitions.Where(
                d => (d.IPAPronunciation is null) || (d.RomanisedPronuncation is null)).ToList();

            if (newDefinitionsList.Count == 0)
            {
                return;
            }

            List<Tuple<String, String>> pronunciations = await Pronouncer.GetDefinitionPronunciations(newDefinitionsList);

            using var context = _linguineDbContextFactory.CreateDbContext();
            context.ChangeTracker.Clear();

            for (int i = 0; i != newDefinitionsList.Count; i++)
            {
                DictionaryDefinition def = newDefinitionsList[i];

                def.IPAPronunciation      = pronunciations[i].Item1;
                def.RomanisedPronuncation = pronunciations[i].Item2;

                context.Update(def);
                await context.SaveChangesAsync();

                context.ChangeTracker.Clear();
            }
        }

        private List<Statement> FromProtoStatements(List<ProtoStatement> protos, TextualMedia tm, int firstChar)
        {
            List<StatementBuilder> builders = protos.Select(p => new StatementBuilder(p)).ToList();

            foreach (StatementBuilder sb in builders)
            {
                sb.Parent = tm;
            }

            SetIndices(builders, firstChar);

            List<Statement> ret = builders.Select(b => b.ToStatement()).ToList();

            return ret;
        }

        private async Task ParseDefinitions(List<Statement> statements)
        {
            if (DefinitionParser is null)
            {
                StartParsingEngine();
            }
            
            if (DefinitionParser is null)
            {
                Log.Fatal("Couldn't start parsing engine!");
                throw new Exception("failed to start parsing engine");
            }

            HashSet<DictionaryDefinition> definitions = StatementManager.GetAllUniqueDefinitions(statements);

            LearnerLevel level  = ConfigManager.Config.GetLearnerLevel();
            LanguageCode native = ConfigManager.Config.Languages.NativeLanguage;

            HashSet<DictionaryDefinition> newDefinitionsSet = 
                ParsedDictionaryDefinitionManager.FilterOutKnown(
                    definitions, level, native);

            HashSet<ParsedDictionaryDefinition> parsedDefinitions = 
                await DefinitionParser.ParseStatementsDefinitions(
                    newDefinitionsSet, level, native);

            ParsedDictionaryDefinitionManager.AddSet(parsedDefinitions);
        }


        private void SetIndices(List<StatementBuilder> builders, int startOfStatementsIndex)
        {
            if (builders.Select(b => b.Parent).Distinct().Count() != 1)
            {
                throw new Exception("something went wrong!");
            }

            String parentText = builders.FirstOrDefault().Parent.Text;

            int ptr = startOfStatementsIndex;

            foreach (StatementBuilder builder in builders)
            {
                builder.FirstCharIndex = parentText.IndexOf(builder.StatementText, ptr);
                builder.LastCharIndex = builder.FirstCharIndex + builder.StatementText.Length - 1;

                ptr = builder.LastCharIndex + 1 ?? throw new Exception();
            }
        }

        private async Task<List<ProtoStatement>?> DoProcessingStep(String text, List<String> context, bool isTail, CancellationToken token)
        {
            if (TextAnalyser is null)
            {
                StartStatementEngine();
            }

            if (TextAnalyser is null)
            {
                Log.Fatal("couldn't start statement engine");
                throw new Exception("couldn't start statement engine");
            }

            if (token.IsCancellationRequested)
            {
                return null;
            }

            List<ProtoStatement>? protos = await TextAnalyser.GenerateStatementsFor(text, context, isTail, token);

            return protos;

        }

        private void StartStatementEngine()
        {
            List<String> dictionaries = ExternalDictionaryManager.AvailableDictionaries();
            if (dictionaries.Count == 0)
            {
                throw new Exception("no dictionary!");
            }
            else if (dictionaries.Count > 1)
            {
                throw new NotImplementedException();
            }
            else
            {
                ExternalDictionary dictionary = ExternalDictionaryManager.GetDictionary(dictionaries[0])
                    ?? throw new Exception();

                APIKeysConfig keys = ConfigManager.Config.APIKeys;

                TextAnalyser = StatementEngineFactory.BuildStatementEngine(dictionary);
            }
        }

        private void StartParsingEngine()
        {
            DefinitionParser = DefinitionParserFactory.BuildParsingEngine();
        }

        private void StartPronunciationEngine()
        {
            Pronouncer = DefinitionPronouncerFactory.BuildPronunciationEngine();
        }

        private List<String> GetPreviousContext(TextualMedia tm)
        {
            Statement? previousStatement = StatementManager.GetLastStatement(tm);

            if (previousStatement is null)
            {
                return FormInitialContext(tm);
            }
            else
            {
                return previousStatement.StatementContext;
            }
        }

        // TODO - translate the word "called" here to the TargetLanguage!
        private List<String> FormInitialContext(TextualMedia tm)
        {
            List<String> ret = new List<String>();

            ret.Add(tm.Description);
            ret.Add("called " + tm.Name);

            return ret;
        }

    }
}

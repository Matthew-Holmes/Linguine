using Infrastructure;
using LearningExtraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Serilog;

namespace Linguine
{

    
    public partial class MainModel
    {

        private ICanAnalyseText?      TextAnalyser     { get; set; }

        private ICanParseDefinitions? DefinitionParser { get; set; }

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

        internal async Task ProcessNextChunkForSession(int sessionID)
        {
            TextualMedia? tm = GetSessionFromID(sessionID)?.TextualMedia ?? null;
            if (tm is null) { return; }

            await ProcessNextChunk(tm);
        }

        internal async Task ProcessNextChunk(TextualMedia tm)
        {

            // determine the chunk of text to process next
            (String? text, List<String>? context, bool isTail, int firstChar) = GetNextChunkInfo(tm);
            if (text is null || context is null) { return; }

            List<ProtoStatement>? builders = await DoProcessingStep(text, context, isTail);

            if (builders is null)
            {
                return;
            }

            List<Statement> statements = FromProtoStatements(builders, tm, firstChar);

            StatementManager.AddStatements(statements);

            if (ConfigManager.Config.LearningForeignLanguage())
            {
                await ParseDefinitions(statements);
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

        private async Task<List<ProtoStatement>?> DoProcessingStep(String text, List<String> context, bool isTail)
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

            List<ProtoStatement> protos = await TextAnalyser.GenerateStatementsFor(text, context, isTail);

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

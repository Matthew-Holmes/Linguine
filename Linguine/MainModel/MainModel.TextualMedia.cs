using Infrastructure;
using System.Collections.Generic;
using DataClasses;
using Config;
using System;
using System.Threading.Tasks;
using System.Linq;
using LearningExtraction;
using Serilog;
using System.Windows.Forms;

namespace Linguine
{
    public partial class MainModel
    {
        public List<string> AvailableTextualMediaNames
        {
            get
            {
                using var context = ReadonlyLinguineFactory.CreateDbContext();
                return SM.Managers!.TextualMedia.AvailableTextualMediaNames(context);
            }
        }

        public LanguageCode Native => ConfigManager.Config.Languages.NativeLanguage;

        internal string? GetFullTextFromSessionID(int sessionId)
        {
            var session = GetSessionFromID(sessionId);

            if (session is null) { return null; };

            return session.TextualMedia.Text;
        }

        internal List<int>? GetSortedStatementStartIndicesFromSessionID(int sessionId)
        {
            var session = GetSessionFromID(sessionId);

            if (session is null) { return null; }

            var ret = SM.Managers!.Statements.StatementStartIndices(session.TextualMedia);

            ret.Sort();

            return ret;
        }


        internal List<Statement>? GetStatementsCoveringRange(int sessionId, int start, int end)
        {
            var session = GetSessionFromID(sessionId);

            if (session is null) { return null; }

            return SM.Managers!.Statements.GetStatementsCoveringRange(session.TextualMedia, start, end);
        }

        internal ParsedDictionaryDefinition? GetParsedDictionaryDefinition(DictionaryDefinition core)
        {
            LanguageCode native = ConfigManager.Config.Languages.NativeLanguage;
            LearnerLevel level  = ConfigManager.Config.GetLearnerLevel();
            return SM.Managers!.ParsedDefinitions.GetParsedDictionaryDefinition(core, level, native);
        }

        internal void DeleteTextualMedia(string selectedTextName)
        {
            using var context = _linguineDbContextFactory.CreateDbContext();
            SM.Managers!.TextualMedia.DeleteByName(selectedTextName, context);
        }

        internal async Task<string> GetBestWordTranslation(
            Statement statement, int defIndex)
        {
            LanguageCode native = ConfigManager.Config.Languages.NativeLanguage;
            List<StatementTranslation> existing = SM.Managers!.Statements.GetTranslations(statement, native);

            if (existing.Count == 0)
            {
                throw new Exception("requires an existing referece translation"); // TODO - fix the architecture so don't need this
            }

            InitialDefinitionAnalyis ida = await SM.Engines.DefinitionResolver.GetInitialAnalysis(existing.First(), defIndex, native);

            return ida.wordBestTranslation; // TODO - flesh this out with more
        }

        internal async Task<string> GetStatementTranslationText(Statement statement)
        {
            using var context = LinguineFactory.CreateDbContext();

            // check for existing translations in the database
            LanguageCode native = ConfigManager.Config.Languages.NativeLanguage;
            List<StatementTranslation> existing = SM.Managers!.Statements.GetTranslations(statement, native);

            if (existing.Count > 0)
            {
                return existing.First().Translation;
            }

            StatementTranslation? generated = await SM.Engines.DefinitionResolver.GetTranslation(statement);

            if (generated is not null) 
            {
                SM.Managers!.Statements.AddTranslation(generated, context);
            }

            if (generated is null)
            {
                Log.Warning("null translation found");
                return "";
            }

            if (generated.Translation == "")
            {
                Log.Warning("empty translation returned");
            }

            return generated.Translation;

        }

        internal async Task<List<Tuple<int, string>>> GetExistingDefinitionKeysAndTexts(string rootedWordText)
        {
            using var context = LinguineFactory.CreateDbContext(); // not thread safe so make a new one here
            

            // TODO - maybe play around with the casing here??
            List<DictionaryDefinition> defs = SM.Managers!.Definitions.TryGetDefinition(rootedWordText);

            if (!ConfigManager.Config.LearningForeignLanguage())
            {
                return defs.Select(def => Tuple.Create(def.DatabasePrimaryKey, def.Definition)).ToList();
            }

            List<ParsedDictionaryDefinition> pdefs = new List<ParsedDictionaryDefinition>();


            HashSet<DictionaryDefinition> toParseNow = new HashSet<DictionaryDefinition>();

            LearnerLevel lvl    = ConfigManager.Config.GetLearnerLevel();
            LanguageCode native = ConfigManager.Config.Languages.NativeLanguage;

            foreach(DictionaryDefinition def in defs)
            {
                ParsedDictionaryDefinition? pdef = SM.Managers!.ParsedDefinitions.GetParsedDictionaryDefinition(def, lvl, native);

                if (pdef is not null) /* already got it */
                {
                    pdefs.Add(pdef);
                } 
                else /* going to have to parse it now */
                {
                    toParseNow.Add(def);
                }
            }

            // get the parsed definition for those that were not parsed


            if (toParseNow.Count > 0)
            {

                HashSet<ParsedDictionaryDefinition> newParsed = await SM.Engines.DefinitionParser
                    .ParseStatementsDefinitions(toParseNow, lvl, native);

                SM.Managers!.ParsedDefinitions.AddSet(newParsed, context);

                pdefs.AddRange(newParsed);
            }

            // get the pronunciation information for those without

            List<DictionaryDefinition> withoutPronunciations = defs.Where(def => def.RomanisedPronuncation is null).ToList();

            List<Tuple<String, String>> pronunciations = await SM.Engines.Pronouncer.GetDefinitionPronunciations(withoutPronunciations);


            context.ChangeTracker.Clear();

            for (int i = 0; i != pronunciations.Count; i++)
            {
                DictionaryDefinition def = withoutPronunciations[i];

                def.IPAPronunciation = pronunciations[i].Item1;
                def.RomanisedPronuncation = pronunciations[i].Item2;

                context.Update(def);
                context.SaveChanges();

                context.ChangeTracker.Clear();
            }

            return pdefs.Select(pdef => Tuple.Create(pdef.DictionaryDefinitionKey, pdef.ParsedDefinition)).ToList();
        }

        internal bool ResolveDefinition(Statement statement, int defIndex, int selectedDefKey)
        {
            DictionaryDefinition? def = SM.Managers!.Definitions.TryGetDefinitionByKey(selectedDefKey);

            if (def is null)
            {
                Log.Warning("failed to find definition when provided with key {defKey}", selectedDefKey);
                return false;
            }

            int rootedTotalUnits = statement.RootedDecomposition.Flattened().Decomposition.Count();
            int level1units      = statement.RootedDecomposition.Decomposition.Count();

            if (rootedTotalUnits != level1units)
            {
                throw new NotImplementedException("need to implement this for multi level decompositions");
            }

            int? statementID = statement.ID;

            if (statementID is null)
            {
                throw new Exception("need to call this on a statement with a record in the database!");
            }

            int statementKey = (int)statementID;

            using var context = LinguineFactory.CreateDbContext();

            StatementDefinitionNode? existing = context.StatementDefinitions
                .Where(sd => sd.CurrentLevel == 1)
                .Where(sd => sd.IndexAtCurrentLevel == defIndex)
                .Where(sd => sd.StatementKey == statementKey)
                .FirstOrDefault();

            EntryMethod method = EntryMethod.User;

            if (existing is not null)
            {
                if (existing.DefinitionKey == selectedDefKey)
                {
                    Log.Information("same definition selected, not changing the database");
                    return true;
                }

                switch (existing.EntryMethod)
                {
                    case EntryMethod.User:
                        method = EntryMethod.UserOverwriteUser;
                        break;
                    case EntryMethod.Machine:
                        method = EntryMethod.UserOverwriteMachine;
                        break;
                    case EntryMethod.UserOverwriteUser:
                        method = EntryMethod.UserOverwriteUser;
                        break;
                    case EntryMethod.UserOverwriteMachine:
                        method = EntryMethod.UserOverwriteUser;
                        break;
                    default:
                        throw new NotImplementedException();
                }

                Log.Information("removing old definition");
                context.Remove(existing);
            }


            StatementDefinitionNode toAdd = new StatementDefinitionNode
            {
                CurrentLevel         = 1,
                IndexAtCurrentLevel  = defIndex,
                DefinitionKey        = def.DatabasePrimaryKey,
                StatementKey         = statementKey,
                EntryMethod          = method,
            };


            context.Add(toAdd);
            context.SaveChanges();

            statement.RootedDecomposition.Decomposition[defIndex].Definition = def; // so shows up in the UI if we have the statement loaded

            return true;
        }

        internal void HideStatementForTesting(Statement parent)
        {
            using var context = LinguineFactory.CreateDbContext();

            StatementDatabaseEntry? entry = context.Statements.Where(s => s.DatabasePrimaryKey == parent.ID).FirstOrDefault();

            if (entry is null)
            {
                Log.Error("tried to hide non-existant statement");
                return;
            }

            entry.IsHiddenFromTesting = true;

            context.Update(entry);

            context.SaveChanges();
        }
    }
}

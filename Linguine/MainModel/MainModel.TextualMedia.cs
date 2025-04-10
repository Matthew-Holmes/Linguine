using Infrastructure;
using System.Collections.Generic;
using DataClasses;
using Config;
using System;
using System.Threading.Tasks;
using System.Linq;
using LearningExtraction;
using Windows.ApplicationModel.Chat;
using Serilog;

namespace Linguine
{
    public partial class MainModel
    {
        public List<string> AvailableTextualMediaNames
        {
            get
            {
                using var context = LinguineFactory.CreateDbContext();
                return TextualMediaManager.AvailableTextualMediaNames(context);
            }
        }

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

            var ret = StatementManager.StatementStartIndices(session.TextualMedia);

            ret.Sort();

            return ret;
        }


        internal List<Statement>? GetStatementsCoveringRange(int sessionId, int start, int end)
        {
            var session = GetSessionFromID(sessionId);

            if (session is null) { return null; }

            return StatementManager.GetStatementsCoveringRange(session.TextualMedia, start, end);
        }

        internal ParsedDictionaryDefinition? GetParsedDictionaryDefinition(DictionaryDefinition core)
        {
            LanguageCode native = ConfigManager.Config.Languages.NativeLanguage;
            LearnerLevel level  = ConfigManager.Config.GetLearnerLevel();
            return ParsedDictionaryDefinitionManager.GetParsedDictionaryDefinition(core, level, native);
        }

        internal void DeleteTextualMedia(string selectedTextName)
        {
            using var context = _linguineDbContextFactory.CreateDbContext();
            TextualMediaManager.DeleteByName(selectedTextName, context);
        }

        internal async Task<string> GetStatementTranslationText(Statement statement)
        {
            // check for existing translations in the database
            LanguageCode native = ConfigManager.Config.Languages.NativeLanguage;
            List<StatementTranslation> existing = StatementManager.GetTranslations(statement, native);

            if (existing.Count > 0)
            {
                return existing.First().Translation;
            }

            if (DefinitionResolver is null)
            {
                StartDefinitionResolutionEngine();
            }

            StatementTranslation? generated = await DefinitionResolver.GetTranslation(statement);

            if (generated is not null) 
            {
                StatementManager.AddTranslation(generated);
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
    }
}

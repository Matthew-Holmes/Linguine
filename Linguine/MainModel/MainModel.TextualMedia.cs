using Infrastructure;
using Linguine.Tabs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

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


    }
}

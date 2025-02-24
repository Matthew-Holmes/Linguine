﻿using Infrastructure;
using System;
using System.Collections.Generic;

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
            // TODO - how to treat the learner level in config - since this will evolve/change
            // so must be in the model somehow?
            LanguageCode native = ConfigManager.NativeLanguage;
            LearnerLevel level  = ConfigManager.GetLearnerLevel(ConfigManager.TargetLanguage); // TODO - just a property
            return ParsedDictionaryDefinitionManager.GetParsedDictionaryDefinition(core, level, native);
        }
          

    }
}

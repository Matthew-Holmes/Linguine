using Infrastructure;
using System.Collections.Generic;

namespace Linguine
{
    public partial class MainModel
    {
        public List<string> AvailableTextualMediaNames
        {
            get => TextualMediaManager.AvailableTextualMediaNames();
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

    }
}

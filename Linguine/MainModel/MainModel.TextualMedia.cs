using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        internal List<int>? GetStatementStartIndicesFromSessionID(int sessionId)
        {
            var session = GetSessionFromID(sessionId);

            if (session is null) { return null; }

            return StatementManager.StatementStartIndices(session.TextualMedia);
        }


        internal List<Statement>? GetStatementsCoveringRange(int sessionId, int start, int end)
        {
            var session = GetSessionFromID(sessionId);

            if (session is null) { return null; }

            return StatementManager.GetStatementsCoveringRange(session.TextualMedia, start, end);
        }

    }
}

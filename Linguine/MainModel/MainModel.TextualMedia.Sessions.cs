using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    public partial class MainModel
    {
        public event EventHandler? SessionsChanged;

        public List<int> ActiveSessionsIDs
        {
            get => TextualMediaSessionManager
                ?.ActiveSessions()
                ?.Select(s => s.DatabasePrimaryKey)
                ?.ToList() 
                ?? new List<int>();
        }

        internal bool StartNewTextualMediaSession(string selectedTextName)
        {
            var tm = TextualMediaManager?.GetByName(selectedTextName) ?? null;

            if (tm is null)
            {
                return false;
            }

            if (TextualMediaSessionManager.NewSession(tm))
            {
                SessionsChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }

            return false;
        }

        internal TextualMediaSession? GetSessionFromID(int sessionID, LinguineContext lg)
        {
            var ret = lg.TextualMediaSessions
                .Where(s => s.DatabasePrimaryKey == sessionID)
                .Include(s => s.TextualMedia)
                .FirstOrDefault();

            return ret;
        }

        internal void CloseSession(int sessionID)
        {
            using LinguineContext lg = new LinguineContext(_linguineConnectionString);
            var session = GetSessionFromID(sessionID, lg); 

            if (session is null) { return; }

            TextualMediaSessionManager.CloseSession(session); // not the end of the world if we don't have it, 
        }

        internal List<Tuple<bool, decimal>>? GetSessionInfoByName(string name)
        {
            TextualMedia? tm = TextualMediaManager?.GetByName(name) ?? null;

            if (tm is null) { return null; }

            return TextualMediaSessionManager?.SessionInfo(tm) ?? null;
        }

        internal bool ActivateExistingSessionFor(string selectedTextName, decimal progress)
        {
            TextualMedia? tm = TextualMediaManager?.GetByName(selectedTextName) ?? null;

            if (tm is null) { return false; }

            bool b = TextualMediaSessionManager?.ActivateExistingSession(tm, progress) ?? false;

            if (b)
            {
                SessionsChanged?.Invoke(this, EventArgs.Empty);
            }
            return b;
        }

        internal int GetCursor(int sessionID)
        {
            using LinguineContext lg = new LinguineContext(_linguineConnectionString);
            return GetSessionFromID(sessionID, lg)?.Cursor ?? 0;
        }

        internal bool CursorMoved(int sessionID, int newCursor)
        {
            using LinguineContext lg = new LinguineContext(_linguineConnectionString);
            var session = GetSessionFromID(sessionID, lg);
            if (session is null) { return false; }

            session.Cursor = newCursor;
            lg.SaveChanges();

            return true;
        }

        internal DateTime WhenLastActive(int sessionID)
        {
            using LinguineContext lg = new LinguineContext(_linguineConnectionString);
            return GetSessionFromID(sessionID, lg)?.LastActive ?? DateTime.MinValue;
        }

    }
}

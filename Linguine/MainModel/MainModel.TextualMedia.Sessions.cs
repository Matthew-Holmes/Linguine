using Infrastructure;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using DataClasses;

namespace Linguine
{
    public partial class MainModel
    {
        public event EventHandler? SessionsChanged;

        public List<int> ActiveSessionsIDs
        {
            get => SM.Managers!.Sessions
                ?.ActiveSessions()
                ?.Select(s => s.DatabasePrimaryKey)
                ?.ToList() 
                ?? new List<int>();
        }

        internal bool StartNewTextualMediaSession(string selectedTextName)
        {
            using var roContext = ReadonlyLinguineFactory.CreateDbContext();

            var tm = SM.Managers!.TextualMedia.GetByName(selectedTextName, roContext) ?? null;

            if (tm is null)
            {
                return false;
            }

            roContext.Dispose();

            using var context = LinguineFactory.CreateDbContext();

            if (SM.Managers!.Sessions.NewSession(tm, context))
            {
                SessionsChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }

            return false;
        }

        internal TextualMediaSession? GetSessionFromID(int sessionID)
        {
            using var context = LinguineFactory.CreateDbContext();
            var ret = context.TextualMediaSessions
                .Where(s => s.DatabasePrimaryKey == sessionID)
                .Include(s => s.TextualMedia)
                .FirstOrDefault();

            return ret;
        }

        internal void CloseSession(int sessionID)
        {
            using var context = LinguineFactory.CreateDbContext();

            var session = GetSessionFromID(sessionID); 

            if (session is null) { return; }

            SM.Managers!.Sessions.CloseSession(session, context); // not the end of the world if we don't have it, 
        }

        internal List<Tuple<bool, decimal>>? GetSessionInfoByName(string name)
        {
            using var context = ReadonlyLinguineFactory.CreateDbContext();
            TextualMedia? tm = SM.Managers!.TextualMedia.GetByName(name, context) ?? null;

            if (tm is null) { return null; }

            using var roContext = LinguineFactory.CreateDbContext();

            return SM.Managers!.Sessions.SessionInfo(tm, roContext) ?? null;
        }

        internal bool ActivateExistingSessionFor(string selectedTextName, decimal progress)
        {
            using var roContext = ReadonlyLinguineFactory.CreateDbContext();

            TextualMedia? tm = SM.Managers!.TextualMedia.GetByName(selectedTextName, roContext) ?? null;

            if (tm is null) { return false; }

            using var context = LinguineFactory.CreateDbContext();

            bool b = SM.Managers!.Sessions.ActivateExistingSession(tm, context, progress);

            if (b)
            {
                SessionsChanged?.Invoke(this, EventArgs.Empty);
            }
            return b;
        }

        internal int GetCursor(int sessionID)
        {
            return GetSessionFromID(sessionID)?.Cursor ?? 0;
        }

        internal bool CursorMoved(int sessionID, int newCursor)
        {
            var session = GetSessionFromID(sessionID);
            if (session is null) { return false; }

            using var context = LinguineFactory.CreateDbContext();

            context.Attach(session);

            session.Cursor = newCursor;
            context.SaveChanges();

            return true;
        }

        internal DateTime WhenLastActive(int sessionID)
        {
            return GetSessionFromID(sessionID)?.LastActive ?? DateTime.MinValue;
        }
    }
}

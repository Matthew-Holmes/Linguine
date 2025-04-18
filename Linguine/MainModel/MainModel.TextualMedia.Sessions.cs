﻿using Infrastructure;
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
            get => TextualMediaSessionManager
                ?.ActiveSessions()
                ?.Select(s => s.DatabasePrimaryKey)
                ?.ToList() 
                ?? new List<int>();
        }

        internal bool StartNewTextualMediaSession(string selectedTextName)
        {
            using var context = LinguineFactory.CreateDbContext();

            var tm = TextualMediaManager?.GetByName(selectedTextName, context) ?? null;

            if (tm is null)
            {
                return false;
            }

            if (TextualMediaSessionManager.NewSession(tm, context))
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
            var session = GetSessionFromID(sessionID); 

            if (session is null) { return; }

            TextualMediaSessionManager.CloseSession(session); // not the end of the world if we don't have it, 
        }

        internal List<Tuple<bool, decimal>>? GetSessionInfoByName(string name)
        {
            using var context = LinguineFactory.CreateDbContext();
            TextualMedia? tm = TextualMediaManager?.GetByName(name, context) ?? null;

            if (tm is null) { return null; }

            return TextualMediaSessionManager?.SessionInfo(tm, context) ?? null;
        }

        internal bool ActivateExistingSessionFor(string selectedTextName, decimal progress)
        {
            using var context = LinguineFactory.CreateDbContext();

            TextualMedia? tm = TextualMediaManager?.GetByName(selectedTextName, context) ?? null;

            if (tm is null) { return false; }

            bool b = TextualMediaSessionManager?.ActivateExistingSession(tm, context, progress) ?? false;

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

using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Helpers;
using Microsoft.VisualBasic;

namespace Infrastructure
{
    public class TextualMediaSessionManager : ManagerBase
    {
        public TextualMediaSessionManager(LinguineDataHandler db) : base(db)
        {
        }

        public void CloseSession(TextualMediaSession session)
        {
            if (session.Active)
            {
                session.LastActive = DateTime.Now;
            }

            session.Active = false;
            _db.SaveChanges();

        }

        public List<TextualMediaSession> ActiveSessions()
        {
            return _db.TextualMediaSessions.Where(tms => tms.Active).ToList();
        }

        public bool NewSession(TextualMedia tm)
        {
            // check if we already have a session with cursor at the start

            TextualMediaSession? alreadyHere = Sessions(tm).Where(s => s.Cursor == 0).FirstOrDefault();

            if (alreadyHere is not null)
            {
                alreadyHere.Active = true;
                alreadyHere.LastActive = DateTime.Now;
                _db.SaveChanges(); // calling this in context where changes are updates the database
                return true;
            }

            TextualMediaSession toAdd = new TextualMediaSession
            {
                Active = true,
                TextualMedia = tm,
                Cursor = 0,
                LastActive = DateTime.Now
            };

            _db.TextualMediaSessions.Add(toAdd);
            _db.SaveChanges();

            return true;

        }

        public List<Tuple<bool,decimal>> SessionInfo(TextualMedia tm)
        {
            // returns if active and progress percentage

            List<TextualMediaSession> sessions = Sessions(tm);

            List<bool> activities = sessions.Select(s => s.Active).ToList();   
            
            List<decimal> progress = sessions.Select(s => GetProgressPercentage(s)).ToList();
            List<decimal> roundedProgress = PercentageHelper.RoundDistinctPercentages(progress);

            return activities.Zip(roundedProgress, (a, p) => new Tuple<bool, decimal>(a, p)).ToList();
        }

        private List<TextualMediaSession> Sessions(TextualMedia tm)
        {
            return _db.TextualMediaSessions.Where(
                s => s.TextualMedia.DatabasePrimaryKey == tm.DatabasePrimaryKey).ToList();
        }

        private void Remove(TextualMediaSession tm)
        {
            _db.TextualMediaSessions.Remove(tm);
            _db.SaveChanges();
        }

        private void TidySessionsFor(TextualMedia tm)
        {
            // don't want multiple cursors pointing at the same location

            List<TextualMediaSession> sessions = Sessions(tm)
                .OrderByDescending(session => session.LastActive).ToList();
            // time order so oldest at the end (so get deleted first)

            List<int> cursors = new List<int>();

            foreach (TextualMediaSession session in sessions)
            {
                if (cursors.Contains(session.Cursor))
                {
                    Remove(session);
                }
                cursors.Add(session.Cursor);
            }
        }

        private decimal GetProgressPercentage(TextualMediaSession session)
        {
            return (decimal) (100.0 * (double)session.Cursor / (double)session.TextualMedia.Text.Length);
        }

        public void Activate(TextualMediaSession tms)
        {
            tms.Active = true;
            tms.LastActive = DateTime.Now;
            _db.SaveChanges();
        }

        public bool ActivateExistingSession(TextualMedia text, decimal progress, decimal proximity = 1.0m)
        {
            var sessions = Sessions(text);

            if (sessions.Count == 0)
            {
                return false;
            }

            List<decimal> progresses = sessions.Select(s => GetProgressPercentage(s)).ToList();

            decimal closest = progresses.OrderBy(item => Math.Abs(progress - item)).First();

            if (Math.Abs(closest - progress) > proximity)
            {
                return false;
            }

            int closestIndex = progresses.IndexOf(closest);

            Activate(sessions[closestIndex]);

            return true;
        }
    }
}

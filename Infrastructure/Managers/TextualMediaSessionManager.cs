using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Helpers;
using Microsoft.VisualBasic;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class TextualMediaSessionManager : ManagerBase
    {
        public TextualMediaSessionManager(String conn) : base(conn)
        {
        }

        public void CloseSession(TextualMediaSession session, LinguineContext lg)
        {
            if (session.Active)
            {
                session.LastActive = DateTime.Now;
            }

            session.Active = false;
            lg.SaveChanges();

        }

        public List<TextualMediaSession> ActiveSessions()
        {
            using LinguineContext lg = Linguine();
            return lg.TextualMediaSessions.Where(tms => tms.Active).ToList();
        }

        public bool NewSession(TextualMedia tm, LinguineContext lg)
        {
            // check if we already have a session with cursor at the start
            TextualMediaSession? alreadyHere = Sessions(tm, lg).Where(s => s.Cursor == 0).FirstOrDefault();
            
            if (alreadyHere is not null)
            {
                alreadyHere.Active = true;
                alreadyHere.LastActive = DateTime.Now;
                lg.SaveChanges(); // calling this in context where changes are updates the database
                return true;
            }

            TextualMediaSession toAdd = new TextualMediaSession
            {
                Active = true,
                TextualMedia = tm,
                Cursor = 0,
                LastActive = DateTime.Now
            };

            lg.TextualMediaSessions.Add(toAdd);
            lg.SaveChanges();

            return true;

        }

        public List<Tuple<bool,decimal>> SessionInfo(TextualMedia tm, LinguineContext lg)
        {
            // returns if active and progress percentage

            TidySessionsFor(tm);

            List<TextualMediaSession> sessions = Sessions(tm, lg);

            List<bool> activities = sessions.Select(s => s.Active).ToList();   
            
            List<decimal> progress = sessions.Select(s => GetProgressPercentage(s)).ToList();
            List<decimal> roundedProgress = PercentageHelper.RoundDistinctPercentages(progress);

            return activities.Zip(roundedProgress, (a, p) => new Tuple<bool, decimal>(a, p)).ToList();
        }

        private List<TextualMediaSession> Sessions(TextualMedia tm, LinguineContext lg)
        {
            return lg.TextualMediaSessions.Where(
                s => s.TextualMedia.DatabasePrimaryKey == tm.DatabasePrimaryKey).ToList();
        }

        private void Remove(TextualMediaSession tm, LinguineContext lg)
        {
            lg.TextualMediaSessions.Remove(tm);
            lg.SaveChanges();
        }

        private void TidySessionsFor(TextualMedia tm)
        {
            // don't want multiple cursors pointing at the same location

            using LinguineContext lg = Linguine();
            List<TextualMediaSession> sessions = Sessions(tm, lg)
                .OrderByDescending(session => session.LastActive).ToList();
            // time order so oldest at the end (so get deleted first)

            List<int> cursors = new List<int>();

            foreach (TextualMediaSession session in sessions)
            {
                if (cursors.Contains(session.Cursor))
                {
                    Remove(session, lg);
                }
                cursors.Add(session.Cursor);
            }
        }

        private decimal GetProgressPercentage(TextualMediaSession session)
        {
            return (decimal) (100.0 * (double)session.Cursor / (double)session.TextualMedia.Text.Length);
        }

        public void Activate(TextualMediaSession tms, LinguineContext lg)
        {
            tms.Active = true;
            tms.LastActive = DateTime.Now;
            lg.SaveChanges();
        }

        public bool ActivateExistingSession(TextualMedia text, decimal progress, LinguineContext lg, decimal proximity = 1.0m)
        {
            var sessions = Sessions(text, lg);

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

            Activate(sessions[closestIndex], lg);

            return true;
        }
    }
}

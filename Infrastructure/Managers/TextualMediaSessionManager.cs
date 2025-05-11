using Helpers;
using DataClasses;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class TextualMediaSessionManager : ManagerBase
    {
        public TextualMediaSessionManager(LinguineReadonlyDbContextFactory dbf) : base(dbf)
        {
        }

        public void CloseSession(TextualMediaSession session, LinguineDbContext context)
        {
            context.Attach(session);

            if (session.Active)
            {
                session.LastActive = DateTime.Now;
            }

            session.Active = false;
            context.SaveChanges();
        }

        public List<TextualMediaSession> ActiveSessions()
        {
            using var context = _dbf.CreateDbContext();
            return context.TextualMediaSessions.Where(tms => tms.Active).ToList();
        }

        public bool NewSession(TextualMedia tm, LinguineDbContext context)
        {
            // check if we already have a session with cursor at the start
            TextualMediaSession? alreadyHere = Sessions(tm, context).Where(s => s.Cursor == 0).FirstOrDefault();

            if (alreadyHere is not null)
            {
                alreadyHere.Active = true;
                alreadyHere.LastActive = DateTime.Now;
                context.SaveChanges(); // calling this in context where changes are updates the database
                return true;
            }

            TextualMediaSession toAdd = new TextualMediaSession
            {
                Active = true,
                TextualMedia = tm,
                Cursor = 0,
                LastActive = DateTime.Now
            };

            context.TextualMediaSessions.Add(toAdd);
            context.SaveChanges();

            return true;

        }

        public List<Tuple<bool,decimal>> SessionInfo(TextualMedia tm, LinguineDbContext context)
        {
            // returns if active and progress percentage

            TidySessionsFor(tm, context);

            List<TextualMediaSession> sessions = Sessions(tm, context);

            List<bool> activities = sessions.Select(s => s.Active).ToList();   
            
            List<decimal> progress = sessions.Select(s => GetProgressPercentage(s)).ToList();
            List<decimal> roundedProgress = PercentageHelper.RoundDistinctPercentages(progress);

            return activities.Zip(roundedProgress, (a, p) => new Tuple<bool, decimal>(a, p)).ToList();
        }

        private List<TextualMediaSession> Sessions(TextualMedia tm, LinguineDbContext context)
        {
            return context.TextualMediaSessions.Include(s => s.TextualMedia).Where(
                s => s.TextualMedia.DatabasePrimaryKey == tm.DatabasePrimaryKey).ToList();
        }

        private List<TextualMediaSession> Sessions(TextualMedia tm, LinguineReadonlyDbContext context)
        {
            throw new NotImplementedException("do we need to be using the with text version?");
            return context.TextualMediaSessions.Where(
                s => s.TextualMedia.DatabasePrimaryKey == tm.DatabasePrimaryKey).ToList();
        }

        private void TidySessionsFor(TextualMedia tm, LinguineDbContext context)
        {
            // don't want multiple cursors pointing at the same location

            List<TextualMediaSession> sessions = Sessions(tm, context)
                .OrderByDescending(session => session.LastActive).ToList();
            // time order so oldest at the end (so get deleted first)

            List<int> cursors = new List<int>();

            foreach (TextualMediaSession session in sessions)
            {
                if (cursors.Contains(session.Cursor))
                {
                    context.TextualMediaSessions.Remove(session);
                }
                cursors.Add(session.Cursor);
            }
        }

        private decimal GetProgressPercentage(TextualMediaSession session)
        {
            return (decimal) (100.0 * (double)session.Cursor / (double)session.TextualMedia.Text.Length);
        }

        public void Activate(TextualMediaSession tms, LinguineDbContext context)
        {
            tms.Active = true;
            tms.LastActive = DateTime.Now;
            context.SaveChanges();
        }

        public bool ActivateExistingSession(TextualMedia text, LinguineDbContext context, decimal progress, decimal proximity = 1.0m)
        {
            var sessions = Sessions(text, context);

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

            Activate(sessions[closestIndex], context);

            return true;
        }
    }
}

using Helpers;
using DataClasses;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure
{
    public class TextualMediaSessionManager : DataManagerBase
    {
        public TextualMediaSessionManager(LinguineReadonlyDbContextFactory dbf) : base(dbf)
        {
        }

        public void CloseText(TextualMedia tm, LinguineDbContext context)
        {
            context.Attach(tm);

            if (tm.IsOpen)
            {
                tm.LastActive = DateTime.Now;
            }

            tm.IsOpen= false;
            context.SaveChanges();
        }

        public List<TextualMedia> ActiveSessions()
        {
            using var context = _dbf.CreateDbContext();
            return context.TextualMedia.Where(tm => tm.IsOpen).ToList();
        }


        public void Open(TextualMedia tm, LinguineDbContext context)
        {
            context.Attach(tm);

            tm.IsOpen  = true;
            tm.LastActive = DateTime.Now;

            context.SaveChanges();
        }
    }
}

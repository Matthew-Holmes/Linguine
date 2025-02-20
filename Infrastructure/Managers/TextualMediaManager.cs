using Infrastructure;

namespace Linguine
{
    public class TextualMediaManager : ManagerBase
    {
        public TextualMediaManager(LinguineDbContextFactory dbf) : base(dbf)
        {
        }

        public void Add(TextualMedia tm)
        {
            using var context = _dbf.CreateDbContext();

            if (AvailableTextualMediaNames(context).Contains(tm.Name))
            {
                throw new ArgumentException("already have a text of this name");
            }
            context.TextualMedia.Add(tm);
            context.SaveChanges();
        }

        public List<String> AvailableTextualMediaNames(LinguineDbContext context)
        {
            return context.TextualMedia.Select(m => m.Name)
                                 .Distinct()
                                 .ToList();
        }

        public bool HaveMediaWithSameDescription(String description, LinguineDbContext context)
        {
            return context.TextualMedia.Where(m => m.Description == description).Any();
        }

        public bool HaveMediaWithSameContent(String text, LinguineDbContext context)
        {
            return context.TextualMedia.Where(m => m.Description == text).Any();
        }

        public TextualMedia? GetByName(string selectedText, LinguineDbContext context)
        {
            var possibilities = context.TextualMedia.Where(m => m.Name == selectedText).ToList();
    
            if (possibilities is null || possibilities.Count == 0)
            {
                return null;
            }

            if (possibilities.Count > 1)
            {
                throw new Exception("multiple texts of same name found - database corrupted!");
            }

            return possibilities.First(); // Warning - Lazy loading will break if this is used later, since dbContext gets disposed
        }
    }
}
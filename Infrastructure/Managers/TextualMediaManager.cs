using DataClasses;

namespace Infrastructure
{
    public class TextualMediaManager : ManagerBase
    {
        public TextualMediaManager(LinguineReadonlyDbContextFactory dbf) : base(dbf)
        {
        }

        public void Add(TextualMedia tm, LinguineDbContext context)
        {
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

        public List<String> AvailableTextualMediaNames(LinguineReadonlyDbContext context)
        {
            return context.TextualMedia.Select(m => m.Name)
                                 .Distinct()
                                 .ToList();
        }


        public bool HaveMediaWithSameDescription(String description, LinguineReadonlyDbContext context)
        {
            return context.TextualMedia.Where(m => m.Description == description).Any();
        }

        public bool HaveMediaWithSameContent(String text, LinguineReadonlyDbContext context)
        {
            return context.TextualMedia.Where(m => m.Description == text).Any();
        }

        public TextualMedia? GetByName(string selectedText, LinguineReadonlyDbContext context)
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

        public void DeleteByName(string selectedTextName, LinguineDbContext context)
        {

            TextualMedia? tm = GetByName(selectedTextName, context);

            if (tm is null)
            {
                return;
            }

            context.Remove(tm);
            context.SaveChanges();
        }
    }
}
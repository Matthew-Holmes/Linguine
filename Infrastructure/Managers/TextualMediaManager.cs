using Infrastructure;
using Infrastructure.Managers;

namespace Linguine
{
    public class TextualMediaManager : ManagerBase
    {
        public TextualMediaManager(LinguineDataHandler db) : base(db)
        {
        }

        public void Add(TextualMedia tm)
        {
            if (AvailableTextualMediaNames().Contains(tm.Name))
            {
                throw new ArgumentException("already have a text of this name");
            }

            _db.TextualMedia.Add(tm);
            _db.SaveChanges();
        }

        public List<String> AvailableTextualMediaNames()
        {
            return _db.TextualMedia.Select(m => m.Name)
                               .Distinct()
                               .ToList();
        }

        public bool HaveMediaWithSameDescription(String description)
        {
            return _db.TextualMedia.Where(m => m.Description == description).Any();
        }

        public bool HaveMediaWithSameContent(String text)
        {
            return _db.TextualMedia.Where(m => m.Description == text).Any();
        }

        public TextualMedia? GetByName(string selectedText)
        {
            var possibilities = _db.TextualMedia.Where(m => m.Name == selectedText).ToList();
    
            if (possibilities is null || possibilities.Count == 0)
            {
                return null;
            }

            if (possibilities.Count > 1)
            {
                throw new Exception("multiple texts of same name found - database corrupted!");
            }

            return possibilities.First();

        }
    }
}
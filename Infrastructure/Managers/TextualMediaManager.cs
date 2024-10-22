using Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Linguine
{
    public class TextualMediaManager : ManagerBase
    {
        public TextualMediaManager(String conn) : base(conn)
        {
        }

        public void Add(TextualMedia tm)
        {
            if (AvailableTextualMediaNames().Contains(tm.Name))
            {
                throw new ArgumentException("already have a text of this name");
            }

            using LinguineContext lg = Linguine();
            lg.TextualMedia.Add(tm);
            lg.SaveChanges();
        }

        public List<String> AvailableTextualMediaNames()
        {
            using LinguineContext lg = Linguine();
            return lg.TextualMedia.Select(m => m.Name)
                               .Distinct()
                               .ToList();
        }

        public bool HaveMediaWithSameDescription(String description)
        {
            using LinguineContext lg = Linguine();
            return lg.TextualMedia.Where(m => m.Description == description).Any();
        }

        public bool HaveMediaWithSameContent(String text)
        {
            using LinguineContext lg = Linguine();
            return lg.TextualMedia.Where(m => m.Description == text).Any();
        }

        public TextualMedia? GetByName(string selectedText, LinguineContext lg)
        {
            var possibilities = lg.TextualMedia.Where(m => m.Name == selectedText).ToList();
    
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
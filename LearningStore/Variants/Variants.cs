using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningStore
{
    public class Variants
    {
        public LanguageCode LanguageCode { get; }
        public String Name { get; }

        private readonly VariantsContext _context;

        public Variants(LanguageCode lc, String name, String ConnectionString)
        {
            LanguageCode = lc;
            Name = name;

            _context = new VariantsContext(ConnectionString);

            if (!_context.Database.CanConnect())
            {
                throw new InvalidOperationException("Cannot connect to the database.");
            }

            // Check if the database is empty
            if (!_context.Variants.Any())
            {
                throw new InvalidOperationException("The database is empty.");
            }
        }

        public IEnumerable<String> GetVariants(String root)
        {
            return _context.Variants
                .Where(v => v.Root.Equals(root))
                .Select(v => v.Variant)
                .Distinct()
                .ToList();
        }

        public IEnumerable<String> GetRoots(String variant)
        {
            return _context.Variants
                .Where(v => v.Variant.Equals(variant))
                .Select(v => v.Root)
                .Distinct()
                .ToList();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

    }
}

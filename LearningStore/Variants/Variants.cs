using Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningStore
{
    public class Variants
    {
        public String Source { get; }

        private LinguineDbContext _db;

        public Variants(String source, LinguineDbContext db)
        {
            Source = source;
            _db = db;
        }

        public IEnumerable<String> GetVariants(String root)
        {
            return _db.Variants
                .Where(v => v.Source == Source)
                .Where(v => v.Root.Contains(root))
                .Select(v => v.Variant)
                .Distinct()
                .ToList();
        }

        public IEnumerable<String> GetRoots(String variant)
        {
            return _db.Variants
                .Where(v => v.Source == Source)
                .Where(v => v.Variant.Contains(variant))
                .Select(v => v.Root)
                .Distinct()
                .ToList();
        }

        internal bool Add(VariantRoot variantRoot, bool save = true)
        {
            // TODO - test
            if (variantRoot.Source !=  Source)
            {
                return false;
            }

            _db.Variants.Add(variantRoot);

            if (save)
            {
                _db.SaveChanges();
            }

            return true;
        }

        internal bool Add(List<VariantRoot> variantRoots)
        {
            // TODO - test
            if (variantRoots.Any(v => v.Source != Source))
            {
                return false;
            }

            foreach (var vr in variantRoots)
            {
                Add(vr, false);
            }

            _db.SaveChanges();

            return true;
        }

        internal bool DuplicateEntries()
        {
            // TODO - test
            return _db.Variants
                      .Where(v => v.Source == Source)
                      .GroupBy(p => new { p.Variant, p.Root })
                      .Where(p => p.Count() > 1)
                      .Any();
        }
    }
}

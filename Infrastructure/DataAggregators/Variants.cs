using DataClasses;

namespace Infrastructure
{
    // TODO - do we really need multiple sources??

    public class Variants
    {
        public String Source { get; }

        private LinguineDbContextFactory _dbf;

        public Variants(String source, LinguineDbContextFactory dbf)
        {
            Source = source;
            _dbf = dbf;
        }

        public IEnumerable<String> GetVariants(String root)
        {
            using var context = _dbf.CreateDbContext();
            return context.Variants
                .Where(v => v.Source == Source)
                .Where(v => v.Root.Contains(root))
                .Select(v => v.Variant)
                .Distinct()
                .ToList();
        }

        public IEnumerable<String> GetRoots(String variant)
        {
            using var context = _dbf.CreateDbContext();
            return context.Variants
                .Where(v => v.Source == Source)
                .Where(v => v.Variant.Contains(variant))
                .Select(v => v.Root)
                .Distinct()
                .ToList();
        }

        internal bool Add(VariantRoot variantRoot, LinguineDbContext context, bool save = true)
        {
            // TODO - test
            if (variantRoot.Source !=  Source)
            {
                return false;
            }

            context.Variants.Add(variantRoot);

            if (save)
            {
                context.SaveChanges();
            }

            return true;
        }

        internal bool Add(List<VariantRoot> variantRoots)
        {
            using var context = _dbf.CreateDbContext();
            // TODO - test
            if (variantRoots.Any(v => v.Source != Source))
            {
                return false;
            }

            foreach (var vr in variantRoots)
            {
                Add(vr, context, false);
            }

            context.SaveChanges();

            return true;
        }

        internal bool DuplicateEntries()
        {
            using var context = _dbf.CreateDbContext();
            // TODO - test
            return context.Variants
                      .Where(v => v.Source == Source)
                      .GroupBy(p => new { p.Variant, p.Root })
                      .Where(p => p.Count() > 1)
                      .Any();
        }
    }
}

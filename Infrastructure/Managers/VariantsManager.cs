using DataClasses;

namespace Infrastructure
{
    public class VariantsManager : DataManagerBase
    {
        public VariantsManager(LinguineReadonlyDbContextFactory dbf) : base(dbf)
        {
        }

        public IEnumerable<String> GetVariants(String root)
        {
            using var context = _dbf.CreateDbContext();
            return context.Variants
                .Where(v => v.Root.Contains(root))
                .Select(v => v.Variant)
                .Distinct()
                .ToList();
        }

        public IEnumerable<String> GetRoots(String variant)
        {
            using var context = _dbf.CreateDbContext();
            return context.Variants
                .Where(v => v.Variant.Contains(variant))
                .Select(v => v.Root)
                .Distinct()
                .ToList();
        }

        internal bool Add(VariantRoot variantRoot, LinguineDbContext context, bool save = true)
        {
            context.Variants.Add(variantRoot);

            if (save)
            {
                context.SaveChanges();
            }

            return true;
        }

        internal bool Add(List<VariantRoot> variantRoots, LinguineDbContext context)
        {
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
                      .GroupBy(p => new { p.Variant, p.Root })
                      .Where(p => p.Count() > 1)
                      .Any();
        }


        public void AddNewVariantsSourceFromCSV(String filename, LinguineDbContext context)
        {
            VariantsCSVParser.ParseVariantsFromCSVToSQLiteAndSave(this, filename, context);

            VerifyIntegrity();

        }

        public void VerifyIntegrity()
        {
           if (DuplicateEntries())
           {
                throw new Exception("Found duplicate rows");
           }
        }
    }
}

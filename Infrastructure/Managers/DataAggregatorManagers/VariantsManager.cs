namespace Infrastructure
{
    public class VariantsManager : ManagerBase
    {
        public VariantsManager(LinguineDbContextFactory dbf) : base(dbf)
        {
        }

        public List<String> AvailableVariantsSources()
        {
            using var context = _dbf.CreateDbContext();
            return context.Variants.Select(v => v.Source)
                               .Distinct()
                               .ToList();
        }

        public Variants? GetVariantsSource(String source)
        {
            if (!AvailableVariantsSources().Contains(source))
            {
                return null;
            }

            return new Variants(source, _dbf);
        }

        public void AddNewVariantsSourceFromCSV(String filename, String source)
        {
            if (AvailableVariantsSources().Contains(source))
            {
                throw new InvalidDataException("naming conflict identified, variants adding aborted");
            }

            Variants target = new Variants(source, _dbf);

            VariantsCSVParser.ParseVariantsFromCSVToSQLiteAndSave(target, filename, source);

            VerifyIntegrity(target);

        }

        public void VerifyIntegrity(Variants variants)
        {
           if (variants.DuplicateEntries())
           {
                throw new Exception("Found duplicate rows");
           }
        }
    }
}

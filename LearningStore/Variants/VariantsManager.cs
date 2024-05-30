using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningStore
{
    public class VariantsManager
    {
        private LinguineDbContext _db;

        public VariantsManager(LinguineDbContext db)
        {
            _db = db;
        }

        public List<String> AvailableVariantsSources()
        {
            return _db.Variants.Select(v => v.Source)
                               .Distinct()
                               .ToList();
        }

        public Variants? GetVariantsSource(String source)
        {
            if (!AvailableVariantsSources().Contains(source))
            {
                return null;
            }

            return new Variants(source, _db);
        }

        public void AddNewVariantsSourceFromCSV(String filename, String source)
        {
            if (AvailableVariantsSources().Contains(source))
            {
                throw new InvalidDataException("naming conflict identified, variants adding aborted");
            }

            Variants target = new Variants(source, _db);

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

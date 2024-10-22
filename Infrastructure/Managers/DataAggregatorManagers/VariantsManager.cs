using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class VariantsManager : ManagerBase
    {
        public VariantsManager(String conn) : base(conn)
        {
        }

        public List<String> AvailableVariantsSources()
        {
            using LinguineContext lg = Linguine();
            return lg.Variants.Select(v => v.Source)
                               .Distinct()
                               .ToList();
        }

        public Variants? GetVariantsSource(String source)
        {
            if (!AvailableVariantsSources().Contains(source))
            {
                return null;
            }
            return new Variants(source, _connectionString);
        }

        public void AddNewVariantsSourceFromCSV(String filename, String source)
        {
            if (AvailableVariantsSources().Contains(source))
            {
                throw new InvalidDataException("naming conflict identified, variants adding aborted");
            }

            Variants target = new Variants(source, _connectionString);

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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class TestRecords
    {
        private ExternalDictionary _dictionary;

        private LinguineDbContextFactory _dbf;

        public TestRecords(ExternalDictionary dictionary, LinguineDbContextFactory dbf)
        {
            _dictionary = dictionary;
            _dbf = dbf;
        }

        public int DistinctDefinitionsTested()
        {
            using var context = _dbf.CreateDbContext();
            return context.TestRecords
                          .DistinctBy(tr => tr.DictionaryDefinitionKey)
                          .Count();
        }
    }
}

using Infrastructure.DataClasses;
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
            return context.TestRecords.Any()
                ? context.TestRecords
                         .GroupBy(tr => tr.DictionaryDefinitionKey)
                         .Count()
                : 0; 
        }

        public IReadOnlyDictionary<int, TestRecord> LatestTestRecords()
        {
            using var context = _dbf.CreateDbContext();

            if (!context.TestRecords.Any())
                return new Dictionary<int, TestRecord>();

            var latestRecords = context.TestRecords
                .GroupBy(tr => tr.DictionaryDefinitionKey)
                .Select(group => group
                    .OrderByDescending(tr => tr.Finished)
                    .First())
                .ToDictionary(tr => tr.DictionaryDefinitionKey);

            return latestRecords;
        }

    }
}

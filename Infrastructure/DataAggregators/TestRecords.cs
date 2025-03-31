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

        public void AddRecord(DictionaryDefinition definition,
            DateTime posed, DateTime answered, DateTime finished,
            bool correct)
        {
            using var context = _dbf.CreateDbContext();

            context.Attach(definition);

            TestRecord toAdd = new TestRecord
            {
                DictionaryDefinitionKey = definition.DatabasePrimaryKey,
                Definition              = definition,
                Posed                   = posed,
                Answered                = answered,
                Finished                = finished,
                Correct                 = correct
            };
            
            context.TestRecords.Add(toAdd);
            context.SaveChanges();
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

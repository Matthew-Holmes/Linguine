using DataClasses;

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

        public List<TestRecord> AllRecordsTimeSorted()
        {
            using var context = _dbf.CreateDbContext();
            return context.TestRecords.ToList(); // TODO - check time sorted
        }

        public List<DictionaryDefinition> DistinctDefinitionsTested()
        {
            using var context = _dbf.CreateDbContext();
            return context.TestRecords.Any()
                ? context.TestRecords
                         .GroupBy(tr => tr.DictionaryDefinitionKey)
                         .Select(grouping => grouping.First().Definition)
                         .ToList() 
                : new List<DictionaryDefinition>();
        }

        public int NumberDistinctDefinitionsTested()
        {
            using var context = _dbf.CreateDbContext();
            return context.TestRecords.Any()
                ? context.TestRecords
                         .GroupBy(tr => tr.DictionaryDefinitionKey)
                         .Count()
                : 0; 
        }


        public TestRecord AddRecord(DictionaryDefinition definition,
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

            return toAdd;
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

        public IReadOnlyDictionary<int, List<TestRecord>> Last5TestRecords()
        {
            using var context = _dbf.CreateDbContext();

            if (!context.TestRecords.Any())
                return new Dictionary<int, List<TestRecord>>();

            // warning - EF couldn't handle grouping in SQL, so had to do `.AsEnumerable()`!
            var latestRecords = context.TestRecords
                .AsEnumerable() 
                .GroupBy(tr => tr.DictionaryDefinitionKey)
                .ToDictionary(
                    group => group.Key,
                    group => group
                        .OrderByDescending(tr => tr.Finished)
                        .Take(5)
                        .ToList()
                );

            return latestRecords;
        }


    }
}

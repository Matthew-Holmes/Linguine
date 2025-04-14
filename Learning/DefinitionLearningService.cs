using Infrastructure;
using Serilog;
using DataClasses;
using Learning.Strategy;
using Infrastructure.Migrations;

namespace Learning
{
    public partial class DefinitionLearningService
    {
        // TODO - more pure functions

        // only need these to be satisfied for free study
        // text based study can work, even for very small texts (?)


        private int  _zipfBinCount      = 1;
        private bool _frozenBinCount    = false;

        private Random rng = new Random();


        private FrequencyData    _freqData;
        private List<TestRecord> _allRecords;

        private Strategist Strategist { get; set; }
        private Tactician Tactician { get; set; }


        public DefinitionLearningService(FrequencyData freqData, List<TestRecord> allRecords)
        {
            _freqData = freqData;
            _allRecords = allRecords;

            VocabModel = new VocabularyModel(freqData, _allRecords);
            Strategist = new Strategist(VocabModel);

            List<List<TestRecord>> sessions = LearningTacticsHelper.GetSessions(_allRecords);
            List<DictionaryDefinition> distinct = DistinctDefinitionTested(_allRecords);

            Tactician = Strategist.BuildModel(sessions, distinct);
        }


        public List<DictionaryDefinition> DistinctDefinitionTested(List<TestRecord> allRecords)
        {
            return allRecords.GroupBy(tr => tr.DictionaryDefinitionKey)
                             .Select(grouping => grouping.First().Definition)
                             .ToList();
        }


        private void ResolveLearningTactics(VocabularyModel vocabModel)
        {

        }


        public int GetHighLearningDefinitionID()
        {
            return Tactician.GetBestDefID();
        }


        public int GetFrequentDefinition(int freq = 5)
        {
            if (DefinitionFrequencyEngine.DefinitionFrequencies is null)
            {
                Log.Error("need to compute the definition frequencies!");
                throw new Exception();
            }

            var eligibleKeys = DefinitionFrequencyEngine.DefinitionFrequencies
                .Where(kvp => kvp.Value > freq) 
                .Select(kvp => kvp.Key)
                .ToList();

            if (eligibleKeys.Count == 0)
                return -1; // No eligible keys found

            Random rnd = new Random();
            int def_key = eligibleKeys[rnd.Next(eligibleKeys.Count)];

            return def_key;
        }




        public IReadOnlyDictionary<int, TestRecord> LatestTestRecords(List<TestRecord> records)
        {
            if (!records.Any())
                return new Dictionary<int, TestRecord>();

            var latestRecords = records
                .GroupBy(tr => tr.DictionaryDefinitionKey)
                .Select(group => group
                    .OrderByDescending(tr => tr.Finished)
                    .First())
                .ToDictionary(tr => tr.DictionaryDefinitionKey);

            return latestRecords;
        }


        public void Inform(TestRecord added)
        {
            Tactician.Inform(added);
        }

    }
}

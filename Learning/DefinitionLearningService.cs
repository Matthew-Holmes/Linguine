using Infrastructure;
using Serilog;
using DataClasses;
using Learning.Strategy;
using Infrastructure.Migrations;

namespace Learning
{
    public partial class DefinitionLearningService
    {
        private Random rng = new Random();

        private List<TestRecord> _allRecords;

        private Strategist Strategist { get; set; }
        private Tactician  Tactician  { get; set; }


        public DefinitionLearningService(FrequencyData freqData, List<TestRecord> allRecords)
        {
            _allRecords = allRecords;

            VocabModel = new VocabularyModel(freqData, _allRecords);
            Strategist = new Strategist(VocabModel);

            List<List<TestRecord>> sessions = LearningTacticsHelper.GetSessions(_allRecords);
            List<DictionaryDefinition> distinct = DistinctDefinitionTested(_allRecords);

            Tactician = Strategist.BuildModel(sessions, distinct);
        }


        public void Inform(TestRecord added)
        {
            Tactician.Inform(added);
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

        private List<DictionaryDefinition> DistinctDefinitionTested(List<TestRecord> allRecords)
        {
            return allRecords.GroupBy(tr => tr.DictionaryDefinitionKey)
                             .Select(grouping => grouping.First().Definition)
                             .ToList();
        }
    }
}

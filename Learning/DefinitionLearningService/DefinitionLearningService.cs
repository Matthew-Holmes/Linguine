using Infrastructure;
using Serilog;
using DataClasses;
using Learning.Strategy;
using Infrastructure.Migrations;

namespace Learning
{
    internal partial class DefinitionLearningService : IDefinitionLearningService
    {
        private Random rng = new Random();

        internal required List<TestRecord> AllRecords  { get; init; }
        internal required FrequencyData    Frequencies { get; init; }
        internal required VocabularyModel  VocabModel  { get; init; }
        internal required Strategist       Strategist  { get; init; }
        internal required Tactician        Tactician   { get; init; }


        public void Inform(TestRecord added)
        {
            Tactician.Inform(added);
        }

        public void Ignore(int defKey)
        {
            Tactician.Ignore(defKey);
        }

        public int GetHighLearningDefinitionID()
        {
            return Tactician.GetBestDefID();
        }

        // TODO - decide what to do here

        public int GetFrequentDefinition(int freq = 5)
        {

            var eligibleKeys = Frequencies.freqs
                .Where(kvp => kvp.Value > freq) 
                .Select(kvp => kvp.Key)
                .ToList();

            if (eligibleKeys.Count == 0)
                return -1; // No eligible keys found

            Random rnd = new Random();
            int def_key = eligibleKeys[rnd.Next(eligibleKeys.Count)];

            return def_key;
        }
    }
}

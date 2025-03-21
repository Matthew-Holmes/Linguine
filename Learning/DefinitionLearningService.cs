using Infrastructure;
using Infrastructure.DataClasses;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning
{
    public class DefinitionLearningService
    {

        // only need these to be satisfied for free study
        // text based study can work, even for very small texts (?)

        // TODO - should these be in the config?
        private int _minWordsProcessed = 1_000;
        private int _minWordsTested    = 50;

        public int VocabTestWordCount => _minWordsTested;

        public bool EnoughDataForWordFrequencies()
        {
            if (DefinitionFrequencyEngine.DefinitionFrequencies is null)
            {
                Log.Error("need to compute the definition frequencies!");
                throw new Exception();
            }

            if (DefinitionFrequencyEngine.DefinitionFrequencies.Count == 0)
            {
                return false;
            }

            return DefinitionFrequencyEngine.DefinitionFrequencies.Values.Sum() >= _minWordsProcessed;
        }

        public bool NeedToBurnInVocabularyData()
        {
            return _testRecords.DistinctDefinitionsTested() < _minWordsTested;
        }


        private ExternalDictionary                _dictionary;
        private TestRecords                       _testRecords;
        private ParsedDictionaryDefinitionManager _pdefManager;
        private StatementManager                  _statementManager;

        public DefinitionLearningService(
            ExternalDictionary                dictionary,
            TestRecords                       testRecords,
            ParsedDictionaryDefinitionManager pdefManager,
            StatementManager                  statementManager)
        {
            _dictionary       = dictionary;
            _testRecords      = testRecords;
            _pdefManager      = pdefManager;
            _statementManager = statementManager;
        }

        public DictionaryDefinition GetRandomDefinition()
        {
            return _dictionary.GetRandomDefinition();
        }

        public DictionaryDefinition GetFrequentDefinition()
        {
            if (DefinitionFrequencyEngine.DefinitionFrequencies is null)
            {
                Log.Error("need to compute the definition frequencies!");
                throw new Exception();
            }

            var eligibleKeys = DefinitionFrequencyEngine.DefinitionFrequencies
                .Where(kvp => kvp.Value > 2) // Filter for counts > 5
                .Select(kvp => kvp.Key)
                .ToList();

            if (eligibleKeys.Count == 0)
                return null; // No eligible keys found

            Random rnd = new Random();
            int def_key = eligibleKeys[rnd.Next(eligibleKeys.Count)];

            DictionaryDefinition? def = _dictionary.TryGetDefinitionByKey(def_key);

            if (def is null)
            {
                Log.Error("couldn't find a definition that was supposed to exist");
                throw new Exception(); // maybe update the frequencies then try again?
            }

            return def;
        }

        public bool AnyDataForWordFrequencies()
        {
            if (DefinitionFrequencyEngine.DefinitionFrequencies is null)
            {
                Log.Error("need to compute the definition frequencies!");
                throw new Exception();
            }

            if (DefinitionFrequencyEngine.DefinitionFrequencies.Count == 0)
            {
                return false;
            }

            return true;
        }

        public DictionaryDefinition GetHighVocabEstimateInformationDefinition()
        {
            // First build the Zipf-Score model, say 5 definitions per 
            // possible zipf score
                // If new data then can access further along the zipf range
                // use most recent test per word??

            // two ways to improve model
                // 1. higher resolution bins
                // 2. more datapoints per bin

            // probabilities decay 

            // break ties by random from new
            // then repeat existing


            throw new NotImplementedException();
        }
    }
}

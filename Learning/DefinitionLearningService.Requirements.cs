using DataClasses;
using Infrastructure;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning
{
    partial class DefinitionLearningService
    {
        private int _minWordsProcessed = 300;
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

            int total = DefinitionFrequencyEngine.DefinitionFrequencies.Values.Sum();
            int unique = DefinitionFrequencyEngine.DefinitionFrequencies.Where(kvp => kvp.Value != 0).Count();

            bool enoughTotalText = total >= _minWordsProcessed;
            bool enoughUniqueWords = unique >= _minWordsTested;

            return enoughTotalText && enoughUniqueWords;
        }

        public bool NeedToBurnInVocabularyData()
        {
            return _allRecords.Count < _minWordsTested;
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

    }
}

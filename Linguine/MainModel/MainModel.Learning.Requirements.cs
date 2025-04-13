using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    partial class MainModel
    {
        public bool NeedToImportADictionary { get; set; } = true;


        internal bool EnoughDataForWordFrequencies()
        {
            if (NeedToImportADictionary) { return false; }

            return DefLearningService.EnoughDataForWordFrequencies();
        }

        internal bool AnyDataForWordFrequencies()
        {
            if (NeedToImportADictionary) { return false; }

            return DefLearningService.AnyDataForWordFrequencies();
        }

        internal bool NeedToBurnInVocabularyData()
        {
            if (NeedToImportADictionary) { return true; }

            return DefLearningService.NeedToBurnInVocabularyData();
        }
    }
}

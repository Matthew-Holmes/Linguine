using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning
{
    public interface IDefinitionLearningService
    {
        public void Inform(TestRecord tr);
        public int GetHighLearningDefinitionID();
        public int GetFrequentDefinition(int freq = 5);
        public int GetInitialVocabEstimationDefinition();
        public DLSRequirements RequirementsMet();
        public int VocabTestWordCount { get; }
        public Tuple<double[], double[]>? GetPKnownByBinnedZipf();

    }
}

using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    // just for holding the method, should never appear as a tactic
    class EventuallyLearntBase : LearningTactic
    {
        public override LearningTactic? Prerequisite => new WasTested();

        public EventuallyLearntBase()
        {
            Constraints = new List<Constraint> { ResolvesLearningCorrect };
        }

        private bool ResolvesLearningCorrect(List<TestRecord> sortedSessionRecords, int defID)
        {
            List<TestRecord> thisDef = sortedSessionRecords.Where(sr => sr.DictionaryDefinitionKey == defID).ToList();

            // we need to have had some incorrect, otherwise is all correct
            if (thisDef.All(tr => tr.Correct == true))
            {
                return false;
            }

            if (thisDef.Count < 2)
            {
                return false;
            }

            return thisDef.Last().Correct;
        }
    }
}

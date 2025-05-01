using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class SingleCorrectTryLongCheck : LearningTactic
    {
        internal override LearningTactic? Prerequisite => new SingleCorrectTry();

        internal SingleCorrectTryLongCheck()
        {
            Constraints = new List<Constraint> { _LongCheck };
            // TODO - change the heirarchy so don't need the repeated magic numbers here
            Thresholds = new List<BasicThresholds> { 
                new BasicThresholds(MaxAverageTime: TimeSpan.FromSeconds(10)),
                new BasicThresholds(MinAverageTime: TimeSpan.FromSeconds(4)) };
        }

        private bool _LongCheck(List<TestRecord> sessionRecords, int defID)
        {
            List<TestRecord> thisDef = sessionRecords.Where(sr => sr.DictionaryDefinitionKey == defID).ToList();

            if (thisDef.Count != 1)
            {
                return false;
            }

            TestRecord response = thisDef.First();

            TimeSpan answering = response.Answered - response.Posed;
            TimeSpan checking = response.Finished - response.Answered;

            return (checking.TotalSeconds / answering.TotalSeconds) > 1.0;

        }
    }
}

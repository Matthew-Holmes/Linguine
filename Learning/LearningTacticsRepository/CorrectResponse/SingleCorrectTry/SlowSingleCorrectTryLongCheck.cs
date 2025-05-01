using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class SlowSingleCorrectTryLongCheck : LearningTactic
    {
        internal override LearningTactic? Prerequisite => new SlowSingleCorrectTry();

        internal SlowSingleCorrectTryLongCheck()
        {
            Constraints = new List<Constraint> { _LongCheck };
        }

        private bool _LongCheck(List<TestRecord> sessionRecords, int defID)
        {
            List<TestRecord> thisDef = sessionRecords.Where(sr => sr.DictionaryDefinitionKey == defID).ToList();

            if (thisDef.Count != 1)
            {
                // need at least two - otherwise its just a single incorrect response
                return false;
            }

            TestRecord response = thisDef.First();

            TimeSpan answering = response.Answered - response.Posed;
            TimeSpan checking = response.Finished - response.Answered;

            return (checking.TotalSeconds / answering.TotalSeconds) > 1.0;

        }
    }
}

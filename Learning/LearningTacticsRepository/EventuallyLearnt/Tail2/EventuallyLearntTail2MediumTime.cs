using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class EventuallyLearntTail2MediumTime : LearningTactic
    {
        public override LearningTactic? Prerequisite => new EventuallyLearntMediumTime();


        public EventuallyLearntTail2MediumTime()
        {
            Constraints = new List<Constraint> { ResolvesTail2 };
        }

        public static bool ResolvesTail2(List<TestRecord> sortedSessionRecords, int defID)
        {
            List<TestRecord> thisDef = sortedSessionRecords.Where(sr => sr.DictionaryDefinitionKey == defID).ToList();


            if (thisDef.Count < 3)
            {
                return false;
            }

            List<TestRecord> tail = thisDef.TakeLast(2).ToList();

            return tail.All(tr => tr.Correct);
        }
    }
}

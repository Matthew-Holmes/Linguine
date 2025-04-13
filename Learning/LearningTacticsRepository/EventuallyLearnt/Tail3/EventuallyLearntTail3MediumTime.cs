using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class EventuallyLearntTail3MediumTime : LearningTactic
    {
        internal override LearningTactic? Prerequisite => new EventuallyLearntTail2MediumTime();


        internal EventuallyLearntTail3MediumTime()
        {
            Constraints = new List<Constraint> { ResolvesTail3 };
        }

        internal static bool ResolvesTail3(List<TestRecord> sortedSessionRecords, int defID)
        {
            List<TestRecord> thisDef = sortedSessionRecords.Where(sr => sr.DictionaryDefinitionKey == defID).ToList();


            if (thisDef.Count < 4)
            {
                return false;
            }

            List<TestRecord> tail = thisDef.TakeLast(3).ToList();

            return tail.All(tr => tr.Correct);
        }
    }
}

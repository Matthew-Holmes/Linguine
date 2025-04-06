using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class EventuallyLearnt : LearningTactic
    {
        public override LearningTactic? Prerequisite => null;

        public EventuallyLearnt()
        {
            NecConstraints = new List<Constraint> { ResolvesLearningCorrect };
        }

        private bool ResolvesLearningCorrect(List<TestRecord> sortedSessionRecords, int defID)
        {
            List<TestRecord> thisDef = sortedSessionRecords.Where(sr => sr.DictionaryDefinitionKey == defID).ToList();

            if (thisDef.Count < 2)
            {
                return false;
            }

            List<TestRecord> tail;

            if (thisDef.Count <= 3)
            {
                tail = thisDef;
            }
            else
            {
                tail = thisDef.TakeLast(3).ToList();
            }

            return tail.All(tr => tr.Correct);
        }

    }
}

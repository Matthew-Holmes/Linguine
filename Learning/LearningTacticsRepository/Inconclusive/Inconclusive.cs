using DataClasses;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class Inconclusive : LearningTactic
    {
        public override LearningTactic? Prerequisite => null;

        public Inconclusive()
        {
            // the user has been correct once, but was not cosistent towards the end of the session

            NecThresholds = new List<BasicThresholds> { new BasicThresholds(MinCorrect: 1) };
            NecConstraints = new List<Constraint>     { LeavesLearningUnresolved };
        }

        private bool LeavesLearningUnresolved(List<TestRecord> sortedSessionRecords, int defID)
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

            return !tail.All(tr => tr.Correct);
        }
    }
}

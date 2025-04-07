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
        public override LearningTactic? Prerequisite => new WasTested();

        public Inconclusive()
        {
            // the user has been correct once, but was not cosistent towards the end of the session

            Thresholds = new List<BasicThresholds> { new BasicThresholds(MinCorrect: 1) };
            Constraints = new List<Constraint>     { LeavesLearningUnresolved };
        }

        private bool LeavesLearningUnresolved(List<TestRecord> sortedSessionRecords, int defID)
        {
            List<TestRecord> thisDef = sortedSessionRecords.Where(sr => sr.DictionaryDefinitionKey == defID).ToList();

            if (thisDef.Count < 2)
            {
                return false;
            }

            // if never correct then wasn't inconclusive
            if (thisDef.All(tr => tr.Correct == false))
            {
                return false;
            }

            return !thisDef.Last().Correct;
        }
    }
}

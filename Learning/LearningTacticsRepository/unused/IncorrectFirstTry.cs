using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class IncorrectFirstTry : LearningTactic
    {
        public override LearningTactic? Prerequisite => null;

        public IncorrectFirstTry()
        {
            NecConstraints = new List<Constraint> { FirstIsIncorrect };
        }

        private bool FirstIsIncorrect(List<TestRecord> timeSortedSessionRecords, int defId)
        {
            List<TestRecord> thisDef = timeSortedSessionRecords.Where(sr => sr.DictionaryDefinitionKey == defId).ToList();

            if (thisDef.Count == 0)
            {
                return false;
            }
            return !thisDef.First().Correct;
        }
    }
}

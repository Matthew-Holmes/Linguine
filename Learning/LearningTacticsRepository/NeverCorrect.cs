using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class NeverCorrect : LearningTactic
    {
        public override LearningTactic? Prerequisite => null;

        public NeverCorrect()
        {
            NecConstraints = new List<Constraint> { NoneCorrect };
        }

        private bool NoneCorrect(List<TestRecord> sessionRecords, int defID)
        {
            List<TestRecord> thisDef = sessionRecords.Where(sr => sr.DictionaryDefinitionKey == defID).ToList();
            return !thisDef.Any(tr => tr.Correct);
        }
    }
}

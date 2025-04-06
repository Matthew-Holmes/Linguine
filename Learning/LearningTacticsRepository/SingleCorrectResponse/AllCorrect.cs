using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class AllCorrect : LearningTactic
    {
        public override LearningTactic? Prerequisite => new WasTested();

        public AllCorrect()
        {
            Constraints = new List<Constraint> { _AllCorrect };
        }

        private bool _AllCorrect(List<TestRecord> sessionRecords, int defID)
        {
            List<TestRecord> thisDef = sessionRecords.Where(sr => sr.DictionaryDefinitionKey == defID).ToList();

            if (thisDef.Count < 2)
            {
                // need at least two - otherwise its just a single incorrect response
                return false;
            }

            return thisDef.All(tr => tr.Correct);
        }
    }
}

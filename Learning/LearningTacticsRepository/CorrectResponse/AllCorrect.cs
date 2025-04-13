using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    internal class AllCorrect : LearningTactic
    {
        internal override LearningTactic? Prerequisite => new WasTested();

        internal AllCorrect()
        {
            Constraints = new List<Constraint> { _AllCorrect };
        }

        private bool _AllCorrect(List<TestRecord> sessionRecords, int defID)
        {
            List<TestRecord> thisDef = sessionRecords.Where(sr => sr.DictionaryDefinitionKey == defID).ToList();

            if (thisDef.Count < 1)
            {
                // need at least two - otherwise its just a single incorrect response
                return false;
            }

            return thisDef.All(tr => tr.Correct);
        }
    }
}

//using DataClasses;
//using Infrastructure;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace Learning.LearningTacticsRepository
//{
//    class CorrectFirstTry : LearningTactic
//    {
//        public override LearningTactic? Prerequisite => null;

//        public CorrectFirstTry()
//        {
//            NecConstraints = new List<Constraint> { FirstIsCorrect };
//        }

//        private bool FirstIsCorrect(List<TestRecord> timeSortedSessionRecords, int defId)
//        {
//            List<TestRecord> thisDef = timeSortedSessionRecords.Where(sr => sr.DictionaryDefinitionKey == defId).ToList();

//            if (thisDef.Count == 0)
//            {
//                return false;
//            }
//            return thisDef.First().Correct;
//        }
//    }
//}

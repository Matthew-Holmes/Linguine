using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class QuickSingleCorrectTry : LearningTactic
    {
        public override LearningTactic? Prerequisite => new CorrectFirstTry();

        public QuickSingleCorrectTry()
        {
            NecThresholds = new List<BasicThresholds> { new BasicThresholds(MaxAverageTime: TimeSpan.FromMinutes(5)) };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class QuickSingleCorrectTry : LearningTactic
    {
        public override LearningTactic? Prerequisite => new SingleCorrectTry();

        public QuickSingleCorrectTry()
        {
            Thresholds = new List<BasicThresholds> { new BasicThresholds(MaxAverageTime: TimeSpan.FromSeconds(7)) };
        }
    }
}

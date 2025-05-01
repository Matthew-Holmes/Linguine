using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class QuickSingleIncorrectTry : LearningTactic
    {
        internal override LearningTactic? Prerequisite => new WasTested();

        internal QuickSingleIncorrectTry()
        {
            Thresholds = new List<BasicThresholds>
            {
                new BasicThresholds(MaxExposures: 1, MinIncorrect: 1),
                new BasicThresholds(MaxAverageTime: TimeSpan.FromSeconds(4))
            };
        }
    }
}

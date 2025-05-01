using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class SlowSingleIncorrectTry : LearningTactic
    {
        internal override LearningTactic? Prerequisite => new WasTested();

        internal SlowSingleIncorrectTry()
        {
            Thresholds = new List<BasicThresholds>
            {
                new BasicThresholds(MaxExposures: 1, MinIncorrect: 1),
                new BasicThresholds(MinAverageTime: TimeSpan.FromSeconds(10)),
            };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class SingleIncorrectTry : LearningTactic
    {
        internal override LearningTactic? Prerequisite => new WasTested();

        internal SingleIncorrectTry()
        {
            Thresholds = new List<BasicThresholds>
            {
                new BasicThresholds(MaxExposures: 1, MinIncorrect: 1),
                new BasicThresholds(MaxAverageTime: TimeSpan.FromSeconds(10)),
                new BasicThresholds(MinAverageTime: TimeSpan.FromSeconds(4))
            };
        }
    }
}

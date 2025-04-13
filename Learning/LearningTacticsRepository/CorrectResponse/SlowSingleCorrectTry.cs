using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class SlowSingleCorrectTry : LearningTactic
    {
        internal override LearningTactic? Prerequisite => new SingleCorrectTry();

        internal SlowSingleCorrectTry()
        {
            Thresholds = new List<BasicThresholds> { new BasicThresholds(MinAverageTime: TimeSpan.FromSeconds(10)) };
        }
    }
}

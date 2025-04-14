using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class SlowlyInconclusive : LearningTactic
    {
        internal override LearningTactic? Prerequisite => new Inconclusive();

        internal SlowlyInconclusive()
        {
            Thresholds = new List<BasicThresholds> { new BasicThresholds(MinTotalTime: TimeSpan.FromSeconds(25)) };
        }
    }
}

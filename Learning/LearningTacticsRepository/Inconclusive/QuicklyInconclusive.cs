using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class QuicklyInconclusive : LearningTactic
    {
        internal override LearningTactic? Prerequisite => new Inconclusive();

        internal QuicklyInconclusive()
        {
            Thresholds = new List<BasicThresholds> { new BasicThresholds(MaxTotalTime: TimeSpan.FromSeconds(20)) };
        }
    }
}

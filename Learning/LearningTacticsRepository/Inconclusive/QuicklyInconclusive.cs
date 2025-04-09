using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class QuicklyInconclusive : LearningTactic
    {
        public override LearningTactic? Prerequisite => new Inconclusive();

        public QuicklyInconclusive()
        {
            Thresholds = new List<BasicThresholds> { new BasicThresholds(MaxTotalTime: TimeSpan.FromSeconds(20)) };
        }
    }
}

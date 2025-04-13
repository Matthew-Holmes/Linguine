using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class EventuallyLearntQuickly : LearningTactic
    {
        internal override LearningTactic? Prerequisite => new EventuallyLearntBase();

        internal EventuallyLearntQuickly()
        {
            Thresholds = new List<BasicThresholds> { new BasicThresholds(MaxTotalTime: TimeSpan.FromSeconds(10)) };
        }
    }
}

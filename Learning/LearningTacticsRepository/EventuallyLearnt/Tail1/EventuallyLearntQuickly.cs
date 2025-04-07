using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class EventuallyLearntQuickly : LearningTactic
    {
        public override LearningTactic? Prerequisite => new EventuallyLearntBase();

        public EventuallyLearntQuickly()
        {
            Thresholds = new List<BasicThresholds> { new BasicThresholds(MaxTotalTime: TimeSpan.FromSeconds(20)) };
        }
    }
}

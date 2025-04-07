using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class EventuallyLearntSlowly : LearningTactic
    {
        public override LearningTactic? Prerequisite => new EventuallyLearntBase();

        public EventuallyLearntSlowly()
        {
            Thresholds = new List<BasicThresholds> { new BasicThresholds(MinTotalTime: TimeSpan.FromSeconds(60)) };
        }
    }
}

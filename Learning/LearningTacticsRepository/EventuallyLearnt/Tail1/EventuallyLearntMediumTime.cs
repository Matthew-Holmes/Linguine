using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class EventuallyLearntMediumTime : LearningTactic
    {
        public override LearningTactic? Prerequisite => new EventuallyLearntBase();

        public EventuallyLearntMediumTime()
        {
            Thresholds = new List<BasicThresholds> { new BasicThresholds(MinTotalTime: TimeSpan.FromSeconds(10), MaxTotalTime: TimeSpan.FromSeconds(25)) };
        }



    }
}

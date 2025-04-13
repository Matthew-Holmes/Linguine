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
        internal override LearningTactic? Prerequisite => new EventuallyLearntBase();

        internal EventuallyLearntSlowly()
        {
            Thresholds = new List<BasicThresholds> { new BasicThresholds(MinTotalTime: TimeSpan.FromSeconds(25)) };
        }
    }
}

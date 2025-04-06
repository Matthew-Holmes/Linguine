using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class SlowSingleCorrectTry : LearningTactic
    {
        public override LearningTactic? Prerequisite => new SingleCorrectTry();

        public SlowSingleCorrectTry()
        {
            NecThresholds = new List<BasicThresholds> { new BasicThresholds(MinAverageTime: TimeSpan.FromSeconds(30)) };
        }
    }
}

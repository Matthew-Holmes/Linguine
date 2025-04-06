using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class WasTested : LearningTactic
    {
        public override LearningTactic? Prerequisite => null;

        public WasTested()
        {
            Thresholds = new List<BasicThresholds> { new BasicThresholds(MinExposures: 1) };
        }
    }
}

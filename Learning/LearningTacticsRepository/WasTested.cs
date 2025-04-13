using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class WasTested : LearningTactic
    {
        internal override LearningTactic? Prerequisite => null;

        internal WasTested()
        {
            Thresholds = new List<BasicThresholds> { new BasicThresholds(MinExposures: 1) };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class LabouriouslyInconclusive : LearningTactic
    {
        internal override LearningTactic? Prerequisite => new Inconclusive();

        internal LabouriouslyInconclusive()
        {
            Thresholds = new List<BasicThresholds> { new BasicThresholds(MinTotalTime: TimeSpan.FromMinutes(1)) };
        }
    }
}

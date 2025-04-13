using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class SingleIncorrectResponse : LearningTactic
    {
        internal override LearningTactic? Prerequisite => new WasTested();

        internal SingleIncorrectResponse()
        {
            Thresholds = new List<BasicThresholds> { new BasicThresholds(MaxExposures: 1, MinIncorrect: 1) };
        }
    }
}

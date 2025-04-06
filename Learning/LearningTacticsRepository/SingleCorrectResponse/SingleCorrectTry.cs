using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class SingleCorrectTry : LearningTactic
    {
        public override LearningTactic? Prerequisite => null;

        public SingleCorrectTry()
        {
            NecThresholds = new List<BasicThresholds> { new BasicThresholds(MaxExposures: 1, MinCorrect: 1) };
        }

    }
}

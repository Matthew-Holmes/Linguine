﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class EventuallyLearntTail3Quickly : LearningTactic
    {
        internal override LearningTactic? Prerequisite => new EventuallyLearntTail2Quickly();

        internal EventuallyLearntTail3Quickly()
        {
            Constraints = new List<Constraint> { EventuallyLearntTail3MediumTime.ResolvesTail3 };
        }
    }
}

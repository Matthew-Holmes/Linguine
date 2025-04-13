using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class EventuallyLearntTail3Slowly : LearningTactic
    {
        internal override LearningTactic? Prerequisite => new EventuallyLearntTail2Slowly();

        internal EventuallyLearntTail3Slowly()
        {
            Constraints = new List<Constraint> { EventuallyLearntTail3MediumTime.ResolvesTail3 };
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class EventuallyLearntTail3Slowly : LearningTactic
    {
        public override LearningTactic? Prerequisite => new EventuallyLearntTail2Slowly();

        public EventuallyLearntTail3Slowly()
        {
            Constraints = new List<Constraint> { EventuallyLearntTail3MediumTime.ResolvesTail3 };
        }
    }
}

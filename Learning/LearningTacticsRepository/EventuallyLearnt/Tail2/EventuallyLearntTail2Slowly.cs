using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class EventuallyLearntTail2Slowly : LearningTactic
    {
        public override LearningTactic? Prerequisite => new EventuallyLearntSlowly();

        public EventuallyLearntTail2Slowly()
        {
            Constraints = new List<Constraint> { EventuallyLearntTail2MediumTime.ResolvesTail2 };
        }
    }
}

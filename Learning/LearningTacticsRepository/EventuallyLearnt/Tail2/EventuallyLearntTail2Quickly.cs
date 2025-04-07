using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.LearningTacticsRepository
{
    class EventuallyLearntTail2Quickly : LearningTactic
    {
        public override LearningTactic? Prerequisite => new EventuallyLearntQuickly();

        public EventuallyLearntTail2Quickly()
        {
            Constraints = new List<Constraint> { EventuallyLearntTail2MediumTime.ResolvesTail2 };
        }
    }
}

using DataClasses;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning
{
    class LearningTactics
    {
        // class for working with single session strategies relating to vocabulary acquisition


        // if no flashcards answered in 5 minutes, assume the session has ended
        private static TimeSpan MinTimeBetweenSessions { get; } = TimeSpan.FromMinutes(5.0);

        private static List<List<TestRecord>> GetSessions(List<TestRecord> total)
        {
            List<List<TestRecord>> ret = new List<List  <TestRecord>>();

            if (total.Count == 0)
            {
                return ret;
            }

            List<TestRecord> sessionToAdd = new List<TestRecord> { total.First() };

            foreach (TestRecord tr in total.Skip(1))
            {
                TimeSpan interval = tr.Finished - sessionToAdd.Last().Posed;
                if (interval <= MinTimeBetweenSessions)
                {
                    // part of existing session
                    sessionToAdd.Add(tr);
                } 
                else
                {
                    // a new session
                    ret.Add(sessionToAdd);
                    sessionToAdd = new List<TestRecord> { tr };
                }
            }

            // final session added
            ret.Add(sessionToAdd);

            return ret;
        }


    }
}

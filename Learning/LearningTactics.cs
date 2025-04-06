using DataClasses;
using Infrastructure;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Learning
{
    class LearningTactics
    {
        // class for working with single session strategies relating to vocabulary acquisition


        // if no flashcards answered in 5 minutes, assume the session has ended
        private static TimeSpan MinTimeBetweenSessions { get; } = TimeSpan.FromMinutes(5.0);
        public static List<List<TestRecord>> GetSessions(List<TestRecord> total)
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

        // the first entry is all the terminal tactics
        // the second all the penultimate etc...
        private List<List<LearningTactic>> LearningTacticHeirarchy;

        public LearningTactics()
        {
            LearningTacticHeirarchy = ResolveLearningTactics();
        }

        public List<LearningTactic?> IdentifyTacticsForSessions(List<List<TestRecord>> sessions, int defID)
        {
            List<LearningTactic?> ret = new List<LearningTactic?>();

            int i = 0;
            bool broke = false;

            foreach (List<TestRecord> session in sessions)
            {
                i++;
                foreach (List<LearningTactic> level in LearningTacticHeirarchy)
                {
                    List<LearningTactic> tacticsUsed = level.Where(lt => lt.UsedThisTactic(session, defID)).ToList();

                    if (tacticsUsed.Count > 1)
                    {
                        Log.Error("invalid tactic logic");
                        throw new Exception();
                    }

                    if (tacticsUsed.Count != 0)
                    {
                        TimeSpan totalTime = TimeSpan.FromTicks(
                            session.Where(tr => tr.DictionaryDefinitionKey == defID)
                                   .Select(tr => tr.Finished - tr.Posed)
                                   .Select(ts => ts.Ticks)
                                   .Select(ticks => Math.Min(ticks, TimeSpan.FromMinutes(5).Ticks))
                                   .Sum());

                        ret.Add(tacticsUsed.First());
                        Log.Information("session {sesssion} for definition {defID} used tactic {tactic}, took {timespan}",
                                        i, defID, tacticsUsed.First().GetType(), totalTime);
                        broke = true; break;
                    } 
                }
                if (!broke)
                {
                    ret.Add(null);
                }
                broke = false;
            }

            return ret;
        }

        private List<List<LearningTactic>> ResolveLearningTactics()
        {
            var baseType = typeof(LearningTactic);

            var derivedTypes = Assembly.GetExecutingAssembly()
                                       .GetTypes()
                                       .Where(t => t.IsClass && !t.IsAbstract && baseType.IsAssignableFrom(t));

            List<LearningTactic> allTactics = new List<LearningTactic>();
            foreach (var type in derivedTypes)
            {
                if (Activator.CreateInstance(type) is LearningTactic tactic)
                {
                    allTactics.Add(tactic);
                }
            }


            List<List<LearningTactic>> heir = new List<List<LearningTactic>>();

            while (allTactics.Count != 0)
            {
                heir.Add(GetAndThenRemoveTerminalTactics(allTactics));
            }

            return heir;

        }

        private List<LearningTactic> GetAndThenRemoveTerminalTactics(List<LearningTactic> allTactics)
        {
            List<LearningTactic> terminal = new List<LearningTactic>();

            foreach (LearningTactic tactic in allTactics)
            {
                bool isTerminal = true;
                foreach ( LearningTactic otherTactic in allTactics)
                {
                    if (otherTactic.Prerequisite is not null)
                    {
                        if (otherTactic.Prerequisite.GetType() == tactic.GetType())
                        {
                            isTerminal = false;
                        }
                    }
                }

                if (isTerminal)
                {
                    terminal.Add(tactic);
                }
            }

            foreach (LearningTactic tactic in terminal)
            {
                allTactics.Remove(tactic);
            }

            return terminal;
        }
    }
}

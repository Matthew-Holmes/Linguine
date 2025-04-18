﻿using DataClasses;
using Learning.LearningTacticsRepository;
using Learning.Tactics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Learning
{
    partial class Tactician
    {
        private MarkovGraph BuildGlobalMarkovGraph(List<TacticTransition> transitions, RewardData? rewardData = null)
        {
            Dictionary<Type, int> tallyFrom = new Dictionary<Type, int>();

            int tallyFromNull = 0;

            Dictionary<Type, Dictionary<Type, int>> tallyTo = new Dictionary<Type, Dictionary<Type, int>>();
            Dictionary<Type, int> tallyToFromNull = new Dictionary<Type, int>();

            Dictionary<Type, Dictionary<Type, double>> tallyCostTo = new Dictionary<Type, Dictionary<Type, double>>();
            Dictionary<Type, double> tallyCostToFromNull = new Dictionary<Type, double>();

            foreach (TacticTransition t in transitions)
            {
                if (t.from is null)
                {
                    tallyFromNull++;
                    if (!tallyToFromNull.ContainsKey(t.to))
                    {
                        tallyToFromNull[t.to] = 1;
                        tallyCostToFromNull[t.to] = Math.Min(t.cost.TotalSeconds, 60);
                    }
                    else
                    {
                        tallyToFromNull[t.to]++;
                        tallyCostToFromNull[t.to] += Math.Min(t.cost.TotalSeconds, 60);
                    }
                }
                else
                {
                    if (!tallyTo.ContainsKey(t.from))
                    {
                        tallyTo[t.from] = new Dictionary<Type, int>();
                        tallyCostTo[t.from] = new Dictionary<Type, double>();
                        tallyFrom[t.from] = 0;
                    }

                    tallyFrom[t.from]++;

                    if (!tallyTo[t.from].ContainsKey(t.to))
                    {
                        tallyTo[t.from][t.to] = 1;
                        tallyCostTo[t.from][t.to] = Math.Min(t.cost.TotalSeconds, 60);
                    }
                    else
                    {
                        tallyTo[t.from][t.to]++;
                        tallyCostTo[t.from][t.to] += Math.Min(t.cost.TotalSeconds, 60);
                    }
                }
            }

            Dictionary<Type, List<MarkovArrow>> arrows = new Dictionary<Type, List<MarkovArrow>>();

            foreach (Type fromType in tallyTo.Keys)
            {
                arrows[fromType] = new List<MarkovArrow>();

                foreach (Type toType in tallyTo[fromType].Keys)
                {
                    double prob = (double)tallyTo[fromType][toType] / (double)tallyFrom[fromType];
                    double avgTime = (double)tallyCostTo[fromType][toType] / (double)tallyTo[fromType][toType];


                    arrows[fromType].Add(new MarkovArrow(toType, prob, avgTime));
                }
            }

            List<MarkovArrow> arrowsFromNull = new List<MarkovArrow>();

            foreach (Type toType in tallyToFromNull.Keys)
            {
                double prob = (double)tallyToFromNull[toType] / (double)tallyFromNull;
                double avgTime = (double)tallyCostToFromNull[toType] / (double)tallyToFromNull[toType];
                arrowsFromNull.Add(new MarkovArrow(toType, prob, avgTime));

                LearningTactic tactic = (LearningTactic)Activator
                    .CreateInstance(toType,
                    BindingFlags.Instance | BindingFlags.NonPublic,
                    null,
                    null,
                    null)!;

            }

            List<bool> edgesFromNullAreCorrect = new List<bool>();

            if (rewardData is null)
            {
                // use population stats
                
                // uses the transition probs for initiasl
                // if no initial reward then infer the probability that we will get to a correct
                // which is the same as knowing the answer in our head
                double initialReward = 0.0;
                foreach (MarkovArrow arrow in arrowsFromNull)
                {
                    LearningTactic toTactic = (LearningTactic)Activator
                        .CreateInstance(arrow.to, 
                                        BindingFlags.Instance | BindingFlags.NonPublic, 
                                        null,
                                        null, 
                                        null)!;

                    bool wasCorrect = Strategist.WasCorrect(toTactic);

                    if (wasCorrect)
                    {
                        initialReward += arrow.prob;
                    }

                    edgesFromNullAreCorrect.Add(wasCorrect);
                }

                rewardData = new RewardData(Strategist.DefaultRewards, initialReward);
            }

            double PFirstTryCorrect = PCorrectFirstTry(arrowsFromNull, edgesFromNullAreCorrect);

            return new MarkovGraph(arrows, arrowsFromNull, edgesFromNullAreCorrect, Strategist.BaseLineReward, rewardData, PFirstTryCorrect);

        }

        private List<TacticTransition> GetAllTransitions(List<List<TestRecord>> sessions)
        {
            List<TacticTransition> transitions = new List<TacticTransition>();

            foreach (List<TestRecord> session in sessions)
            {
                transitions.AddRange(GetTransitions(session));
            }

            return transitions;
        }

        private List<TacticTransition> GetTransitions(List<TestRecord> session)
        {
            List<TestRecord> runningSession = new List<TestRecord>();
            Dictionary<int, LearningTactic> lastTactic = new Dictionary<int, LearningTactic>();

            List<TacticTransition> ret = new List<TacticTransition>();

            foreach (TestRecord tr in session)
            {
                runningSession.Add(tr);

                int defKey = tr.DictionaryDefinitionKey;

                LearningTactic? tactic = Strategist.TacticsHelper.IdentityTacticForSession(
                    runningSession, defKey);

                if (tactic is null)
                {
                    throw new Exception("every tested definition should have a matching tactic!");
                }

                TimeSpan cost = tr.Finished - tr.Posed;
                Type? from = null;
                Type to = tactic.GetType();

                if (lastTactic.ContainsKey(defKey))
                {
                    from = lastTactic[defKey].GetType();
                }
                TacticTransition toAdd = new TacticTransition(from, to, cost);
                ret.Add(toAdd);

                lastTactic[defKey] = tactic;
            }

            return ret;
        }

    }
}

using Config;
using DataClasses;
using Learning.Solver;
using Learning.Strategy;
using Learning.Tactics;
using MathNet.Numerics;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Learning
{
    internal partial class Tactician
    {
        private Strategist  Strategist { get; init; }
        private MarkovGraph GlobalMarkovGraph { get; set; }

        private ConcurrentDictionary<int, Tuple<double[], double[]>> RewardCostArrays { get; } = new ConcurrentDictionary<int, Tuple<double[], double[]>>();
        

        private double LookAheadDays { get; set; } = 1.0;

        internal Tactician(Strategist strat, List<List<TestRecord>> sessions)
        {
            Strategist = strat;

            List<TacticTransition> allTransitions = GetAllTransitions(sessions);

            GlobalMarkovGraph = BuildGlobalMarkovGraph(allTransitions);

            MarkovGraphPlotter.SaveMarkovPlot(GlobalMarkovGraph); // just for debug

            

            BeginInitialisingTwistScores();
        }

        internal (IReadOnlyDictionary<Type, double>, double) GetRewardForFinalState(int defKey)
        {
            // pKnown is the vacuous final state - i.e. do nothing
            double pKnown = Strategist.GetExistingPKnown(defKey, LookAheadDays);

            if (!Strategist.LastTacticUsedForDefinition.ContainsKey(defKey))
            {
                return (Strategist.DefaultRewards, pKnown); // uses population averages
            }

            double currentReward = Strategist.GetExistingPKnown(defKey, LookAheadDays);

            FollowingSessionDatum? input = Strategist.GetCurrentRewardFeatures(defKey, LookAheadDays);

            if (input is null)
            {
                throw new Exception("requested reward for definition without observed sessions");
            }

            Dictionary<Type, double> ret = new Dictionary<Type, double>();

            foreach (Type tacticType in Strategist.TacticsUsed)
            {
                // predict the probability we'll know it in LookAheadDays
                // if tacticType tactic is deployed

                FollowingSessionDatum newInput = input with
                    {
                        sessionTacticType = tacticType,
                        intervalDays = LookAheadDays
                        /* note - here no (now - then) so the probabilities will increase from the pKnow as time passes */
                    };

                double newReward = Strategist.PredictProbability(newInput);

                ret[tacticType] = newReward;
            }

            return (ret.AsReadOnly(), pKnown);
        }

        internal void PlotMDP(int key, string filename, string filenameExploded)
        {
            (var rewards, double pKnown) = GetRewardForFinalState(key);

            RewardData rData = new RewardData(rewards, pKnown);

            MarkovGraph adjusted = GlobalMarkovGraph with { rewardData = rData };

            adjusted = UpdateInitialProbs(adjusted, pKnown);
            MarkovGraph pruned = MarkovGraphTransformer.Prune(adjusted);
            
            MarkovGraphPlotter.SaveMarkovPlot(pruned, filename);
            MarkovGraphPlotter.SaveExplodedMarkovPlot(pruned, filenameExploded);
        }

    }
}

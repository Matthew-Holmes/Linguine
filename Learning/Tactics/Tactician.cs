using Config;
using DataClasses;
using Learning.Strategy;
using Learning.Tactics;
using MathNet.Numerics;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Learning
{
    public partial class Tactician
    {
        private Strategist Strategist { get; init; }
        private MarkovGraph MarkovGraph { get; set; }

        private double LookAheadDays { get; set; } = 1.0;

        public Tactician(Strategist strat)
        {
            Strategist = strat;
        }

        public (IReadOnlyDictionary<Type, double>, double) GetRewardForFinalState(int defKey)
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

        internal void PlotMDP(int key, string filename)
        {
            (var rewards, double pKnown) = GetRewardForFinalState(key);

            RewardData rData = new RewardData(rewards, pKnown);

            MarkovGraph adjusted = MarkovGraph with { rewardData = rData };

            MarkovGraphPlotter.SaveMarkovPlot(adjusted, filename);
        }
    }
}

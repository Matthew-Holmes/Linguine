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
    partial class Tactician
    {
        private Strategist Strategist { get; init; }
        private MarkovGraph MarkovGraph { get; set; }

        private double LookAheadDays { get; set; }

        public Tactician(Strategist strat)
        {
            Strategist = strat;
        }

        public IReadOnlyDictionary<Type, double> GetRewardAtTerminationFor(int defKey)
        {
            if (!Strategist.LastTacticUsedForDefinition.ContainsKey(defKey))
            {
                return Strategist.DefaultRewards; // uses population averages
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
                    };

                double newReward = Strategist.PredictProbability(newInput);

                ret[tacticType] = newReward;
            }
            return ret.AsReadOnly();
        }
    }
}

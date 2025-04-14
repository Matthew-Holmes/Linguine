using DataClasses;
using Learning.Strategy;
using Learning.Tactics;
using MathNet.Numerics.Integration;
using OxyPlot;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Learning
{
    internal partial class Tactician
    {
        private int FreqCap      { get; set; }
        private int FreqCapIndex { get; set; } = 20; // cap the freq scoring by that of the 20th most frequent 



        private void UpdateTwistScores(int lastDefTestedKey)
        {
            FollowingSessionDatum? input = Strategist.GetCurrentRewardFeatures(lastDefTestedKey, LookAheadDays);

            double stickReward;

            Type currentState = CurrentTacticalState[lastDefTestedKey];

            if (!Strategist.DefaultRewards.ContainsKey(currentState))
            {
                // not modelled so just use the baseline
                stickReward = Strategist.BaseLineReward;
            }
            else 
            {
                if (input is null)
                {
                    // no history use population averages
                    stickReward = Strategist.DefaultRewards[currentState];
                }
                else
                {
                    input = input with
                    {
                        sessionTacticType = currentState,
                        intervalDays      = LookAheadDays
                    };
                    stickReward = Strategist.PredictProbability(input);
                }
            }

            // now predict the expected reward if we twist
            // explore each markov arrow from our current state

            double twistReward = 0.0;
            double cost = 0.0;

            // this is the update method, so we know we aren't in the null state

            List<MarkovArrow> options = new List<MarkovArrow>();

            if (GlobalMarkovGraph.directedEdges.ContainsKey(currentState))
            {
                options = GlobalMarkovGraph.directedEdges[currentState];
            }

            if (options.Count == 0) { CurrentTwistScores[lastDefTestedKey] = 0.0; return; }

            foreach (MarkovArrow arrow in options)
            {
                cost += arrow.prob * arrow.costSeconds;

                if (input is not null)
                {
                    FollowingSessionDatum twistInput = input with
                    {
                        sessionTacticType = arrow.to,
                        intervalDays      = LookAheadDays
                    };

                    if (GlobalMarkovGraph.rewardData.rewards.ContainsKey(arrow.to))
                    {
                        twistReward += arrow.prob * Strategist.PredictProbability(twistInput);
                    }
                    else
                    {
                        twistReward += arrow.prob * GlobalMarkovGraph.avgReward;
                    }

                } else
                {
                    if (!Strategist.DefaultRewards.ContainsKey(currentState))
                    {

                        twistReward += Strategist.BaseLineReward * arrow.prob;
                    }
                    else
                    {
                        twistReward += Strategist.DefaultRewards[arrow.to] * arrow.prob;
                    }
                }
            }
            // warning - now favour negative with high cost over negative with low cost!
            // TODO - solve the MDP using Dinkelbach method of average gain to mitigate this

            double rewardDeltaPerCost = (twistReward - stickReward) / cost;

            CurrentTwistScores[lastDefTestedKey] = rewardDeltaPerCost * Math.Min(FreqCap, Strategist.VocabModel.WordFrequencies[lastDefTestedKey]);
        }

        private void InitialiseTwistScores()
        {
            CurrentTwistScores = new Dictionary<int, double>();

            FreqCap = Strategist.VocabModel.WordFrequencies
                .OrderByDescending(kv => kv.Value)
                .Take(FreqCapIndex)
                .Last().Value;

            (double baseTwistReward, double baseCost) = GetBaseGraphRewardCostFromNull();
            double basePCorrect = BaseGraphPCorrectFirstTry();

            foreach (var kvp in Strategist.VocabModel.WordFrequencies)
            {
                double stickReward;
                double twistReward = 0.0;
                double cost        = 0.0;

                if (kvp.Value == 0)
                {
                    CurrentTwistScores[kvp.Key] = 0.0; // TODO - handle this case
                    continue;
                }

                FollowingSessionDatum? knowItTodayInput = Strategist.GetCurrentRewardFeatures(kvp.Key, LookAheadDays);

                if (knowItTodayInput is null /* no previous examples of testing this */)
                {
                    // if no session data use the base graph
                    // but initialise the null reward with our vocab model's probability
                    stickReward = Strategist.VocabModel.PKnownWithError[kvp.Key].Item1;
                    twistReward = baseTwistReward;
                    cost        = baseCost;
                }
                else
                {
                    stickReward = Strategist.PredictProbability(knowItTodayInput);

                    // TODO - adjust the probabilities of the edges from initial node 
                    // so that the correct ones sum to the stick reward

                    MarkovGraph localMarkov = UpdateInitialProbs(GlobalMarkovGraph, basePCorrect, stickReward);

                    foreach (MarkovArrow arrow in localMarkov.edgesFromNull) 
                    { 
                        // the correct global arrows from null sum to basePCorrect
                        // we want these correct local arrows to sum to the stick reward in probability

                        FollowingSessionDatum twistInput = knowItTodayInput with
                        {
                            sessionTacticType = arrow.to,
                            intervalDays = LookAheadDays /* no past if twist now */
                        };

                        if (GlobalMarkovGraph.rewardData.rewards.ContainsKey(arrow.to))
                        {
                            twistReward +=arrow.prob * Strategist.PredictProbability(twistInput);
                        }
                        else
                        {
                            twistReward += arrow.prob * GlobalMarkovGraph.avgReward;
                        }

                        cost += arrow.prob * arrow.costSeconds;
                    }
                }

                double rewardDeltaPerCost = (twistReward - stickReward) / cost;

                CurrentTwistScores[kvp.Key] = rewardDeltaPerCost * Math.Min(kvp.Value, FreqCap);
            }
        }

        private MarkovGraph UpdateInitialProbs(
            MarkovGraph baseMarkovGraph,
            double basePCorrect,
            double newPCorrect)
        {
            List<MarkovArrow> scaledEdgesFromNull = new List<MarkovArrow>();

            for (int i = 0; i != baseMarkovGraph.edgesFromNull.Count; i++)
            {
                MarkovArrow arrow = baseMarkovGraph.edgesFromNull[i];
                bool arrowIsCorrect = baseMarkovGraph.edgeFromNullIsCorrect[i];

                // the correct global arrows from null sum to basePCorrect
                // we want these correct local arrows to sum to the stick reward in probability

                double correctSF   =         newPCorrect / (basePCorrect + 0.000001);
                double incorrectSF = (1.0 - newPCorrect) / (1.0000001 - basePCorrect); // add a bit of an eps

                double scaledProb = arrowIsCorrect ? arrow.prob * correctSF : arrow.prob * incorrectSF;

                scaledEdgesFromNull.Add(arrow with { prob = scaledProb });
            }

            return baseMarkovGraph with { edgesFromNull = scaledEdgesFromNull };
        }

        private double BaseGraphPCorrectFirstTry()
        {
            double ret = 0.0;

            for (int i = 0; i != GlobalMarkovGraph.edgesFromNull.Count; i++)
            {
                MarkovArrow arrow   = GlobalMarkovGraph.edgesFromNull[i];
                bool arrowIsCorrect = GlobalMarkovGraph.edgeFromNullIsCorrect[i];

                if (arrowIsCorrect)
                {
                    ret += arrow.prob;
                }
            }

            return ret;
        }

        private (double reward, double cost) GetBaseGraphRewardCostFromNull()
        {
            // TODO - cache this in the markov graph?

            double reward = 0.0;
            double cost = 0.0;

            foreach (MarkovArrow arrow in GlobalMarkovGraph.edgesFromNull)
            {
                if (GlobalMarkovGraph.rewardData.rewards.ContainsKey(arrow.to))
                {
                    reward += arrow.prob * GlobalMarkovGraph.rewardData.rewards[arrow.to];
                }
                else
                {
                    reward += arrow.prob * GlobalMarkovGraph.avgReward;
                }

                cost += arrow.prob * arrow.costSeconds;
            }

            return (reward, cost);
        }
    }
}

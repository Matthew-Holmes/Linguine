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
using System.Runtime;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Learning
{
    public partial class Tactician
    {
        private List<TestRecord>? CurrentSession { get; set; }
        private Dictionary<int, Type>? CurrentTacticalState { get; set; } = null;
        private Dictionary<int, double>? CurrentTwistScores { get; set; } = null; // "stick or twist"

        private int FreqCap { get; set; }

        // TODO - base the index on the estimated vocab size when we have that, maybe sqrt()?
        private int FreqCapIndex { get; set; } = 20; // cap the freq scoring by that of the 20th most frequent 


        Dictionary<int, int> CoolOff { get; set; } = new Dictionary<int, int>();
        Dictionary<int, int> CoolOffMax { get; set; } = new Dictionary<int, int>();

        internal int GetBestDefID()
        {
            int ret = CurrentTwistScores
                .Where(kv => !CoolOff.ContainsKey(kv.Key))
                .OrderByDescending(kv => kv.Value)
                .First().Key;

            Log.Information("found id with twist score of {value}", CurrentTwistScores[ret]);

            if (CurrentTacticalState?.ContainsKey(ret) ?? false)
            {
                Log.Information("was in state {state}", CurrentTacticalState[ret].Name.Split('.').Last());
            }

            return ret;
        }

        public void Inform(TestRecord tr)
        {
            if (CurrentSession is null || tr.Posed - CurrentSession.Last().Finished > LearningTacticsHelper.MinTimeBetweenSessions)
            {
                CurrentSession = new List<TestRecord> { tr };
                CurrentTacticalState = new Dictionary<int, Type>();

                CoolOffMax = new Dictionary<int, int>();
                CoolOff = new Dictionary<int, int>();
                // TODO - how to update twist scores without deleting init??
            }
            else
            {
                CurrentSession.Add(tr);
            }

            int k = tr.DictionaryDefinitionKey;


            if (tr.Correct)
            {
                if (!CoolOffMax.ContainsKey(k))
                {
                    CoolOff[k] = 0;
                    CoolOffMax[k] = 1;
                }

                CoolOffMax[k]++; CoolOffMax[k]++;
                CoolOff[k] = CoolOffMax[k];
            } else
            {
                CoolOff.Remove(k);
                CoolOffMax[k] = 0;
            }

            foreach (var l in CoolOff.Keys)
            {
                CoolOff[l]--;
                if (CoolOff[l] <= 0)
                {
                    CoolOff.Remove(l);
                }
            }

            UpdateTacticalState();
        }

        private void UpdateTacticalState()
        {
            // now just use the last def id
            // in the future if we have cooloff tactics, then this will need to be more advanced

            int lastDefTestedKey = CurrentSession.Last().DictionaryDefinitionKey;

            Type? tacticType = Strategist.TacticsHelper.IdentityTacticForSession(CurrentSession, lastDefTestedKey)?.GetType() ?? null;

            if (tacticType is null)
            {
                throw new Exception("couldn't identify a tactic for a tested definition");
            }

            CurrentTacticalState[lastDefTestedKey] = tacticType;

            UpdateTwistScores(lastDefTestedKey);
        }

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

            if (MarkovGraph.directedEdges.ContainsKey(currentState))
            {
                options = MarkovGraph.directedEdges[currentState];
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

                    if (MarkovGraph.rewardData.rewards.ContainsKey(arrow.to))
                    {
                        twistReward += arrow.prob * Strategist.PredictProbability(twistInput);
                    }
                    else
                    {
                        twistReward += arrow.prob * MarkovGraph.avgReward;
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

            foreach (var kvp in Strategist.VocabModel.WordFrequencies)
            {
                double stickReward;
                double twistReward = 0.0;
                double cost = 0.0;

                if (kvp.Value == 0)
                {
                    CurrentTwistScores[kvp.Key] = kvp.Value;
                    continue;
                }

                FollowingSessionDatum? input = Strategist.GetCurrentRewardFeatures(kvp.Key, LookAheadDays);

                if (input is null)
                {
                    // if no session data use the base graph
                    // but initialise the null reward with our vocab model's probability
                    stickReward = Strategist.VocabModel.PKnownWithError[kvp.Key].Item1;

                    foreach (MarkovArrow arrow in MarkovGraph.edgesFromNull)
                    {
                        if (MarkovGraph.rewardData.rewards.ContainsKey(arrow.to))
                        {
                            twistReward += arrow.prob * MarkovGraph.rewardData.rewards[arrow.to];
                        }
                        else
                        {
                            twistReward += arrow.prob * MarkovGraph.avgReward;
                        }

                        cost += arrow.prob * arrow.costSeconds;
                    }
                }
                else
                {
                    stickReward = Strategist.PredictProbability(input);

                    foreach (MarkovArrow arrow in MarkovGraph.edgesFromNull)
                    {
                        FollowingSessionDatum twistInput = input with
                        {
                            sessionTacticType = arrow.to,
                            intervalDays = LookAheadDays
                        };

                        if (MarkovGraph.rewardData.rewards.ContainsKey(arrow.to))
                        {
                            twistReward += arrow.prob * Strategist.PredictProbability(twistInput);
                        }
                        else
                        {
                            twistReward += arrow.prob * MarkovGraph.avgReward;
                        }

                        cost += arrow.prob * arrow.costSeconds;
                    }
                }


                double rewardDeltaPerCost = (twistReward - stickReward) / cost;

                CurrentTwistScores[kvp.Key] = rewardDeltaPerCost * Math.Min(kvp.Value, FreqCap);

            }
        }
    }
}

using Learning.Solver;
using Learning.Strategy;
using Learning.Tactics;
using System.Collections.Concurrent;
using System.Diagnostics;

namespace Learning
{
    internal partial class Tactician
    {
        private int FreqCap      { get; set; }
        private int FreqCapIndex { get; set; } = 20; // cap the freq scoring by that of the 20th most frequent 

        private IReadOnlyDictionary<String, int> Indices { get; set; }

        // TODO - get rid of the transition data from the reward arrays
        // pass an ordering to the Dinklebach

        private void UpdateTwistScores(int lastDefTestedKey)
        {
            Type currentState = CurrentTacticalState![lastDefTestedKey];

            String lookup = currentState.ToString().Split('.').Last();

            if (!Indices.ContainsKey(lookup))
            {
                return;
                // not modelled this state because it hasn't appeared in training data - oh well, it will eventually show up...
            } 

            if (!RewardCostArrays.ContainsKey(lastDefTestedKey))
            {
                return; // edge case due to parallelism, its not ideal, but rare enough to not bother handling
            }

            int index = Indices[lookup];

            RewardsCosts arrs = RewardCostArrays[lastDefTestedKey];

            double Er = arrs.rewards[index];
            double Ec = arrs.costs[index];

            double gain = Ec == 0 ? 0 : Er / Ec;
            
            CurrentTwistScores![lastDefTestedKey] = gain * Math.Min(FreqCap, Strategist.VocabModel.WordFrequencies[lastDefTestedKey]);
        }

        private void BeginInitialisingTwistScores(CancellationTokenSource cts)
        {
            CurrentTwistScores = new ConcurrentDictionary<int, double>();

            FreqCap = Strategist.VocabModel.WordFrequencies
                .OrderByDescending(kv => kv.Value)
                .Take(FreqCapIndex)
                .Last().Value;

            var rng = new Random(); // randomise so as we compute them we aren't being only shown 
                                    // words begining with a, the b, then c etc
            var nonZeros = Strategist.VocabModel.WordFrequencies
                .Where(kvp => kvp.Value > 0)
                .OrderBy(_ => rng.Next())
                .ToList();

            int totalStates = Indices.Keys.Count();

            Task.Run(() =>
            {
                try
                {
                    int total = nonZeros.Count();
                    int completed = 0;

                    Parallel.ForEach(nonZeros, new ParallelOptions
                    {
                        MaxDegreeOfParallelism = Environment.ProcessorCount - 1,
                        CancellationToken = cts.Token
                    }, kvp =>
                    {

                        cts.Token.ThrowIfCancellationRequested();

                        InitMarkovData(kvp.Key, kvp.Value, totalStates);

                        int current = Interlocked.Increment(ref completed);
                        double percent = (double)current / total * 100;
                        Debug.WriteLine($"Progress: {percent:F2}%");
                    });
                }
                catch (OperationCanceledException)
                {
                    Debug.WriteLine("Operation was canceled.");
                }
            }, cts.Token);

        }

        private void InitMarkovData(int defKey, int freq, int totalStates)
        {
            FollowingSessionDatum? knowItTodayInput = Strategist.GetCurrentRewardFeatures(defKey, LookAheadDays);

            MarkovGraph localGraph;

            if (knowItTodayInput is null /* no previous examples of testing this */)
            {
                // TODO - investigate thread safety here
                double pKnown = Strategist.VocabModel.PKnownWithError[defKey].Item1;

                // TODO - we might be able to optimise this?
                localGraph = UpdateInitialProbs(GlobalMarkovGraph, pKnown);
            }
            else
            {
                (var rewards, double pKnown) = GetRewardForFinalState(defKey);

                RewardData rData = new RewardData(rewards, pKnown);

                MarkovGraph adjustedReward = GlobalMarkovGraph with { rewardData = rData };

                localGraph = UpdateInitialProbs(adjustedReward, pKnown);
            }

            MarkovGraph         pruned   = MarkovGraphTransformer.Prune(localGraph);
            ExplodedMarkovGraph exploded = MarkovGraphTransformer.Explode(pruned);
            ExplodedMarkovData  rawData  = MarkovGraphTransformer.ToData(exploded);

            (double[] Er, double[] Ec, bool[] _, int startIndex) = Dinkelbach.GetCostRewardExpectionasAndIsTerminated(rawData);

            // TODO - filter for the indices that only correspond to true nodes, not transitions, use indices to build the arrays

            double startGain = Ec[startIndex] == 0 ? 0.0 : Er[startIndex] / Ec[startIndex];

            CurrentTwistScores![defKey] = Math.Min(freq, FreqCap) * startGain;

            RewardsCosts toAdd = new RewardsCosts(Er, Ec, totalStates);

            if (toAdd.rewards.Length != toAdd.costs.Length)
            {
                throw new Exception("lengths mismach in reward cost arrays!");
            }

            if (toAdd.costs.Length != toAdd.cnt)
            {
                // TODO - make the datatype so these checks are implicit!
                throw new Exception("lengths didn't match the expected value in reward cost arrays");
            }

            RewardCostArrays[defKey] = toAdd;
        }

        private MarkovGraph UpdateInitialProbs(
            MarkovGraph baseMarkovGraph, double newPCorrect)
        {
            List<MarkovArrow> scaledEdgesFromNull = new List<MarkovArrow>();
            double basePCorrect = baseMarkovGraph.PCorrectFirstTry;

            for (int i = 0; i != baseMarkovGraph.edgesFromNull.Count; i++)
            {
                MarkovArrow arrow   = baseMarkovGraph.edgesFromNull[i];
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


        private double PCorrectFirstTry(List<MarkovArrow> edgesFromNull, List<bool> edgeFromNullIsCorrect)
        {
            double ret = 0.0;

            for (int i = 0; i != edgesFromNull.Count; i++)
            {
                MarkovArrow arrow   = edgesFromNull[i];
                bool arrowIsCorrect = edgeFromNullIsCorrect[i];

                if (arrowIsCorrect)
                {
                    ret += arrow.prob;
                }
            }

            return ret;
        }
    }
}

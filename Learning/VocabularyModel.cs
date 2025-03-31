using Infrastructure.DataClasses;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace Learning
{
    class VocabularyModel
    {
        IReadOnlyDictionary<int, int>        WordFrequencies;
        IReadOnlyDictionary<int, TestRecord> LatestTestRecords;

        public VocabularyModel(
            IReadOnlyDictionary<int, int> wordFrequencies, 
            IReadOnlyDictionary<int, TestRecord> latestTestRecords)
        {
            WordFrequencies = wordFrequencies;
            LatestTestRecords = latestTestRecords;
        }

        public IReadOnlyDictionary<int, Tuple<double,double>> GetPKnownWithError()
        {
            // use a kernel
                // TODO 
                // loop through kernel parameters (linear for now, could be in frequency space though)
                // reduce kernel while preserving monotonicity
            // if monotonicity not possible with large kernel then so be it
                // set a max threshold

            // best case would be using true isotonic regression...
            // Isotone Optimization in R: Pool-Adjacent-Violators Algorithm(PAVA) and Active Set Methods
            // looks like a bit too much work

            // modelling as estimating the mean of a Bernoulli
            // this is good, because we get high error around p=0.5
            // which intuitively is the region we are most interested in

            // TODO - update the kernel if bad monotonicity score!

            int totalWordsCnt = WordFrequencies.Count;
            double maxKernelSigma = (double)totalWordsCnt / 10.0;

            int nonZeroWordCnt = WordFrequencies.Select(kvp => kvp.Value != 0).Count();
            double initialKernelSigma = 3.0 * ((double)totalWordsCnt / (double)nonZeroWordCnt);

            double sigma = initialKernelSigma;
            Log.Information("sigma is {sigma}", sigma);
            
            List<Tuple<int, double>> sortedRanks = ComputeRanksSorted(WordFrequencies);

            // for now we use a _/\_ kernel, with total non-zero width sigma
            Func<double, double, double, double> kernel = (x, y, s) => Math.Max(1.0 - (Math.Abs(x - y) / s), 0);

            int left = 0;

            Dictionary<int, Tuple<double, double>> ret = new Dictionary<int, Tuple<double,double>>();

            foreach (Tuple<int, double> t in sortedRanks)
            {
                if (LatestTestRecords.ContainsKey(t.Item1))
                {
                    // no error because we know it
                    // maybe later we can use historic records to estimate error?
                    ret[t.Item1] = Tuple.Create(LatestTestRecords[t.Item1].Correct ? 1.0 : 0.0, 0.0);
                    continue;
                }

                double rank = t.Item2;

                int i = left;

                while (i < WordFrequencies.Count)
                {
                    double w = kernel(rank, sortedRanks[i].Item2, sigma);
                    
                    if (w <= 0.0001) { 
                        left++; i++;
                    } else
                    {
                        break;
                    }
                }

                if (i == WordFrequencies.Count)
                {
                    Log.Warning("overran doing p known");
                    ret[t.Item1] = Tuple.Create(0.0, 1.0);
                }


                double totalWeight = 0.0;
                double sumWeightsSquared = 0.0;
                double muhat = 0.0;

                while (i < WordFrequencies.Count)
                {
                    int wordID = sortedRanks[i].Item1;
                    double wordRank = sortedRanks[i].Item2;

                    double w = kernel(rank, wordRank, sigma);

                    if (w <= 0.0001)
                    {
                        break;
                    }

                    if (LatestTestRecords.ContainsKey(wordID))
                    {
                        double yn = LatestTestRecords[wordID].Correct ? 1.0 : 0.0;

                        totalWeight += w;
                        sumWeightsSquared += w * w;
                        muhat += yn * w;
                    }

                    i++;
                }

                if (totalWeight <= 0.00001)
                {
                    Log.Warning("no weight - adjust kernel sigma!");
                    ret[t.Item1] = Tuple.Create(0.0, 1.0);
                    continue;
                }

                muhat = muhat / totalWeight;
                sumWeightsSquared = sumWeightsSquared / (totalWeight * totalWeight);

                double se = Math.Sqrt(muhat * (1.0 - muhat) * sumWeightsSquared);

                ret[t.Item1] = Tuple.Create(muhat, se);
            }

            return ret.AsReadOnly();
        }


        public static List<Tuple<int, double>> ComputeRanksSorted(IReadOnlyDictionary<int, int> wordFrequencies)
        {
            var sorted = wordFrequencies
                .OrderByDescending(kvp => kvp.Value)
                .ToList();

            List<Tuple<int, double>> ranks = new List<Tuple<int, double>>();

            int currentRank = 1;
            int i = 0;

            while (i < sorted.Count)
            {
                int freq = sorted[i].Value;

                // find all items with the same frequency
                int j = i;
                while (j < sorted.Count && sorted[j].Value == freq)
                    j++;

                // number of tied items
                int count = j - i;

                double meanRank = (2 * currentRank + count - 1) / 2.0;

                for (int k = i; k < j; k++)
                {
                    int wordId = sorted[k].Key;
                    ranks.Add(Tuple.Create(sorted[i].Key, meanRank));
                }

                currentRank += count;
                i = j;
            }

            return ranks;
        }
    }
}

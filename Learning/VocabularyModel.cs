using Infrastructure.DataClasses;
using Serilog;

namespace Learning
{
    public class VocabularyModel
    {
        public IReadOnlyDictionary<int, int> WordFrequencies { get; set; }
        IReadOnlyDictionary<int, TestRecord> LatestTestRecords;
        IReadOnlyDictionary<int, double>     ZipfScores;
        double ZipfHi; double ZipfLo;

        public IReadOnlyDictionary<int, Tuple<double, double>>? PKnownWithError { get; set; }

        public VocabularyModel(
            IReadOnlyDictionary<int, int> wordFrequencies, 
            IReadOnlyDictionary<int, TestRecord> latestTestRecords,
            IReadOnlyDictionary<int, double> zipfScores,
            double zipfHi,
            double zipfLo)
        {
            WordFrequencies = wordFrequencies;
            LatestTestRecords = latestTestRecords;
            ZipfScores = zipfScores;
            ZipfHi = zipfHi;
            ZipfLo = zipfLo;
        }

        public Tuple<double[], double[]>? GetPKnownByBinnedZipf()
        {

            if (PKnownWithError is null) { ComputeGetPKnownWithError(); }

            if (PKnownWithError is null)
            {
                throw new Exception("failed to compute p knowns and errors");
            }

            // cube root so that we aim for n bins with n^2 items in each
            int initialBinCount = (int)Math.Pow(
                (double)(LatestTestRecords.Count), 1.0 / 3.0);

            // using Zipf scores so these will be fine
            List<double> binSnapValue = new List<double> { 0.05, 0.1, 0.5, 1.0, 2.0, 3.0, 4.0, 5.0 };

            double snapStarter = (ZipfHi - ZipfLo) / initialBinCount;
            double binSnap = 1.0;
            foreach (double d in binSnapValue)
            {
                if (d > snapStarter)
                {
                    binSnap = d; break;
                }
            }

            double binMid = 0;
            List<Tuple<double, double>> bins = new List<Tuple<double, double>>();

            while (true)
            {
                if (binMid > ZipfHi)
                {
                    break;
                }

                if (binMid + binSnap / 2.0 > ZipfLo)
                {
                    bins.Add(Tuple.Create(binMid - binSnap / 2.0, binMid + binSnap / 2.0));
                }

                binMid += binSnap;
            }

            var binPknowns = new Dictionary<int, List<double>>(); // temp list of values per bin
            var binSes     = new Dictionary<int, List<double>>(); // temp list of values per bin

            foreach (var kvp in ZipfScores)
            {
                double score = kvp.Value;

                for (int i = 0; i < bins.Count; i++)
                {
                    var (low, high) = bins[i];
                    if (score >= low && score < high)
                    {
                        if (!binPknowns.ContainsKey(i))
                        {
                            binPknowns[i] = new List<double>();
                            binSes[i]     = new List<double>();
                        }

                        double pKnown; double se;

                        (pKnown, se) = PKnownWithError[kvp.Key];

                        binPknowns[i].Add(pKnown);
                        binSes[i].Add(se);
                        break;
                    }
                }
            }

            var binStats = new Dictionary<int, Tuple<double, double>>(); // bin index -> (mean, std dev)

            foreach (var kvp in binPknowns)
            {
                var values = kvp.Value;
                var mean = values.Average();

                List<double> ses = binSes[kvp.Key];
                // don't divide by sample size since these were bootstrapped
                double stdDev = Math.Sqrt(ses.Select(v => Math.Pow(v, 2)).Average());

                double mid = (bins[kvp.Key].Item2 + bins[kvp.Key].Item1) / 2.0;

                binStats[kvp.Key] = Tuple.Create(mean, stdDev);

                Log.Information("estimated p known for Zipf: {Zipf}, to be {mean} with stddev {stddev}", mid, mean, stdDev);
            }

            var midpoints = new List<double>();
            var means     = new List<double>();

            foreach (var kvp in binStats.OrderBy(kvp => kvp.Key))
            {
                var (low, high) = bins[kvp.Key];
                var midpoint = (low + high) / 2.0;
                var mean = kvp.Value.Item1;

                midpoints.Add(midpoint);
                means.Add(mean);
            }

            return Tuple.Create(midpoints.ToArray(), means.ToArray());

        }

        public void ComputeGetPKnownWithError()
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

            int nonZeroWordCnt = WordFrequencies.Where(kvp => kvp.Value != 0).Count();
            double initialKernelSigma = 3.0 * ((double)totalWordsCnt / (double)nonZeroWordCnt);

            double sigma = initialKernelSigma;
            Log.Information("sigma is {sigma}", sigma);
            
            List<Tuple<int, double>> sortedRanks = ComputeRanksSorted(WordFrequencies);

            // for now we use a _/\_ kernel, with total non-zero width sigma
            Func<double, double, double, double> kernel = (x, y, s) => Math.Max(1.0 - (Math.Abs(x - y) / s), 0);

            int left = 0;

            Dictionary<int, Tuple<double, double>> ret = new Dictionary<int, Tuple<double,double>>();

            double thisRank = 0.0;
            double cacheMean = 0.0; double cacheStd = 0.0;

            foreach ((int index, double rank) in sortedRanks)
            {

                // optimisation
                if (Math.Abs(rank - thisRank) < 0.25) /* ranks either natural numbers or n.5 */
                {
                    ret[index] = Tuple.Create(cacheMean, cacheStd);
                    continue;
                }
                else
                {
                    thisRank = rank;
                }

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
                    ret[index] = Tuple.Create(0.0, 1.0);
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
                    ret[index] = Tuple.Create(0.0, 1.0);
                    continue;
                }

                muhat = muhat / totalWeight;
                sumWeightsSquared = sumWeightsSquared / (totalWeight * totalWeight);

                double se = Math.Sqrt(muhat * (1.0 - muhat) * sumWeightsSquared);

                ret[index] = Tuple.Create(muhat, se);

                cacheMean = muhat;
                cacheStd = se;
            }

            // overwrite the values we know correctly
            foreach (var kvp in LatestTestRecords) { 

                // no error because we know it
                // maybe later we can use historic records to estimate error?
                ret[kvp.Key] = Tuple.Create(kvp.Value.Correct ? 1.0 : 0.0, 0.0);
                continue;
            }

            PKnownWithError = ret.AsReadOnly();
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
                    ranks.Add(Tuple.Create(wordId, meanRank));
                }

                currentRank += count;
                i = j;
            }

            return ranks;
        }
    }
}

using DataClasses;
using Infrastructure;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning
{
    partial class DefinitionLearningService
    {
        private VocabularyModel VocabModel { get; init; }

        public Tuple<double[], double[]> GetPKnownByBinnedZipf()
        {
            return VocabModel.GetPKnownByBinnedZipf();
        }

        public int GetInitialVocabEstimationDefinition()
        {
            // adaptive binning approach to get good coverage
            int chosenId;

            if (DefinitionFrequencyEngine.DefinitionZipfScores is null)
            {
                Log.Error("need to compute the definition Zipf scores");
                throw new Exception();
            }

            IReadOnlyDictionary<int, TestRecord> latest = LatestTestRecords(_allRecords);

            IReadOnlyDictionary<int, double> zipfScores = DefinitionFrequencyEngine.DefinitionZipfScores;

            if (zipfScores == null || zipfScores.Count == 0)
            {
                Log.Error("no data!");
                throw new Exception();
            }


            double minZipf = DefinitionFrequencyEngine.ZipfLo;
            double maxZipf = DefinitionFrequencyEngine.ZipfHi;

            double binWidth = (maxZipf - minZipf) / _zipfBinCount;

            var bins = Enumerable.Range(0, _zipfBinCount)
                .ToDictionary(i => i, _ => new List<int>());

            foreach (var (id, tr) in latest)
            {
                double zipfScore = zipfScores[id];
                int binIndex = (int)((zipfScore - minZipf) / binWidth);
                if (binIndex == _zipfBinCount) binIndex--; // handle max edge case
                if (binIndex < 0) continue; // below ZipfLo, i.e. never seen
                bins[binIndex].Add(id);
            }

            // check if all bins have at least `binCount` items
            if (!_frozenBinCount && bins.All(b => b.Value.Count >= _zipfBinCount))
            {
                _zipfBinCount++;
            }

            binWidth = (maxZipf - minZipf) / _zipfBinCount;

            int leastPopulatedBindex = bins
                   .OrderBy(b => b.Value.Count)
                   .First().Key;

            var candidates = zipfScores.Where(
                kvp => (int)((kvp.Value - minZipf) / binWidth) == leastPopulatedBindex).ToList();


            if (candidates.Count() == 0)
            {
                _zipfBinCount--;
                _frozenBinCount = true;

                return GetInitialVocabEstimationDefinition();
            }

            candidates = candidates.Where(kvp => !latest.ContainsKey(kvp.Key)).ToList();

            if (candidates.Count == 0)
            {
                // have filled our bins a decent amount, lets just randomly sample
                int ret = GetFrequentDefinition(1);

                int cnt = 0;
                while (latest.ContainsKey(ret))
                {
                    ret = GetFrequentDefinition(1);
                    cnt++;
                    if (cnt > _minWordsTested)
                    {
                        Log.Warning("couldn't find a definition to test, even though we should have enough data");
                        ret = -1;
                        break;
                    }
                }

                Log.Information("took {count} cycles to find a unseen word", cnt);

                return ret;
            }

            chosenId = candidates[rng.Next(candidates.Count())].Key;

            return chosenId;
        }



    }
}

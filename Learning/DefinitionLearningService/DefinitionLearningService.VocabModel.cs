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
        private int  _zipfBinCount    = 1;
        private bool _frozenBinCount  = false;

        public Tuple<double[], double[]>? GetPKnownByBinnedZipf()
        {
            return VocabModel.GetPKnownByBinnedZipf();
        }

        public int GetInitialVocabEstimationDefinition()
        {
            // adaptive binning approach to get good coverage
            int chosenId;

            IReadOnlyDictionary<int, TestRecord> latest = LatestTestRecords(AllRecords);

            IReadOnlyDictionary<int, double> zipfScores = Frequencies.zipfs;

            if (zipfScores == null || zipfScores.Count == 0)
            {
                Log.Error("no data!");
                throw new Exception();
            }

            double minZipf = Frequencies.zipfLo;
            double maxZipf = Frequencies.zipfHi;

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
                }

                Log.Information("took {count} cycles to find a unseen word", cnt);

                return ret;
            }

            chosenId = candidates[rng.Next(candidates.Count())].Key;

            return chosenId;
        }

        private IReadOnlyDictionary<int, TestRecord> LatestTestRecords(List<TestRecord> records)
        {
            if (!records.Any())
                return new Dictionary<int, TestRecord>();

            var latestRecords = records
                .GroupBy(tr => tr.DictionaryDefinitionKey)
                .Select(group => group
                    .OrderByDescending(tr => tr.Finished)
                    .First())
                .ToDictionary(tr => tr.DictionaryDefinitionKey);

            return latestRecords;
        }
    }
}

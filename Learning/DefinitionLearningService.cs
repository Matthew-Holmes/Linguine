using Infrastructure;
using Serilog;
using DataClasses;
using Learning.Strategy;
using Infrastructure.Migrations;

namespace Learning
{
    public class DefinitionLearningService
    {
        // TODO - more pure functions

        // only need these to be satisfied for free study
        // text based study can work, even for very small texts (?)

        // TODO - should these be in the config?
        private int  _minWordsProcessed = 300; // TODO - increase later
        private int  _minWordsTested    = 50;
        private int  _zipfBinCount      = 1;
        private bool _frozenBinCount    = false;

        private Random rng = new Random();
        public int VocabTestWordCount => _minWordsTested;

        public bool EnoughDataForWordFrequencies()
        {
            if (DefinitionFrequencyEngine.DefinitionFrequencies is null)
            {
                Log.Error("need to compute the definition frequencies!");
                throw new Exception();
            }

            if (DefinitionFrequencyEngine.DefinitionFrequencies.Count == 0)
            {
                return false;
            }

            int total  = DefinitionFrequencyEngine.DefinitionFrequencies.Values.Sum();
            int unique = DefinitionFrequencyEngine.DefinitionFrequencies.Where(kvp => kvp.Value != 0).Count();

            bool enoughTotalText   = total  >= _minWordsProcessed;
            bool enoughUniqueWords = unique >= _minWordsTested;

            return enoughTotalText && enoughUniqueWords;
        }

        public bool NeedToBurnInVocabularyData()
        {
            return _allRecords.Count < _minWordsTested;
        }

        public List<DictionaryDefinition> DistinctDefinitionTested(List<TestRecord> allRecords)
        {
            return allRecords.GroupBy(tr => tr.DictionaryDefinitionKey)
                             .Select(grouping => grouping.First().Definition)
                             .ToList();
        }

        private FrequencyData    _freqData;
        private List<TestRecord> _allRecords;

        private Strategist Strategist { get; set; }
        private Tactician Tactician { get; set; }
        private VocabularyModel VocabModel { get; init; }


        public DefinitionLearningService(FrequencyData freqData, List<TestRecord> allRecords)
        {
            _freqData = freqData;
            _allRecords = allRecords;

            VocabModel = new VocabularyModel(freqData, _allRecords);

            ResolveLearningTactics(VocabModel);

        }

        public Tuple<double[], double[]> GetPKnownByBinnedZipf()
        {
            return VocabModel.GetPKnownByBinnedZipf();
        }


        private void ResolveLearningTactics(VocabularyModel vocabModel)
        {
            LearningTacticsHelper tactics = new LearningTacticsHelper();

            List<List<TestRecord>>     sessions = LearningTacticsHelper.GetSessions(_allRecords);
            List<DictionaryDefinition> distinct = DistinctDefinitionTested(_allRecords);

            foreach (DictionaryDefinition def in distinct)
            {
                tactics.IdentifyTacticsForSessions(sessions, def.DatabasePrimaryKey);
            }

            Strategist strategist = new Strategist(vocabModel);


            Tactician = strategist.BuildModel(sessions, distinct);

            Strategist = strategist;
        }


        public int GetHighLearningDefinitionID()
        {
            return Tactician.GetBestDefID();
        }


        public int GetFrequentDefinition(int freq = 5)
        {
            if (DefinitionFrequencyEngine.DefinitionFrequencies is null)
            {
                Log.Error("need to compute the definition frequencies!");
                throw new Exception();
            }

            var eligibleKeys = DefinitionFrequencyEngine.DefinitionFrequencies
                .Where(kvp => kvp.Value > freq) 
                .Select(kvp => kvp.Key)
                .ToList();

            if (eligibleKeys.Count == 0)
                return -1; // No eligible keys found

            Random rnd = new Random();
            int def_key = eligibleKeys[rnd.Next(eligibleKeys.Count)];

            return def_key;
        }

        public bool AnyDataForWordFrequencies()
        {
            if (DefinitionFrequencyEngine.DefinitionFrequencies is null)
            {
                Log.Error("need to compute the definition frequencies!");
                throw new Exception();
            }

            if (DefinitionFrequencyEngine.DefinitionFrequencies.Count == 0)
            {
                return false;
            }

            return true;
        }


        public IReadOnlyDictionary<int, TestRecord> LatestTestRecords(List<TestRecord> records)
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

        public void Inform(TestRecord added)
        {
            Tactician.Inform(added);
        }

    }
}

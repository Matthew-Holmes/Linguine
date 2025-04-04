using Infrastructure;
using Serilog;
using DataClasses;

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
            return _testRecords.DistinctDefinitionsTested() < _minWordsTested;
        }


        private ExternalDictionary                _dictionary;
        private TestRecords                       _testRecords;
        private ParsedDictionaryDefinitionManager _pdefManager;
        private StatementManager                  _statementManager;

        public DefinitionLearningService(
            ExternalDictionary                dictionary,
            TestRecords                       testRecords,
            ParsedDictionaryDefinitionManager pdefManager,
            StatementManager                  statementManager)
        {
            _dictionary       = dictionary;
            _testRecords      = testRecords;
            _pdefManager      = pdefManager;
            _statementManager = statementManager;
        }

        public DictionaryDefinition GetRandomDefinition()
        {
            return _dictionary.GetRandomDefinition();
        }

        public DictionaryDefinition GetFrequentDefinition(int freq = 5)
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
                return null; // No eligible keys found

            Random rnd = new Random();
            int def_key = eligibleKeys[rnd.Next(eligibleKeys.Count)];

            DictionaryDefinition? def = _dictionary.TryGetDefinitionByKey(def_key);

            if (def is null)
            {
                Log.Error("couldn't find a definition that was supposed to exist");
                throw new Exception(); // maybe update the frequencies then try again?
            }

            return def;
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

        public DictionaryDefinition GetHighLearningDefinition(VocabularyModel model, int topK = 5)
        {
            // pass vocab model since we know that this must have all the required data

            if (model.PKnownWithError is null)
            {
                model.ComputePKnownWithError();
            }

            IReadOnlyDictionary<int, double>? pKnown = model.PKnownWithError?
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Item1);

            if (pKnown is null)
            {
                throw new Exception("failed to compute word known probabilities");
            }

            Dictionary<int, double> expectedUnknown = model.WordFrequencies
                .Where(kvp => pKnown.ContainsKey(kvp.Key))
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value * (1.0 - pKnown[kvp.Key])
                 );

            //var debug = expectedUnknown.OrderByDescending(kvp => kvp.Value);

            var topKKeys = expectedUnknown
                .OrderByDescending(kvp => kvp.Value)                     // primary sort by value
                .ThenBy(kvp => rng.Next())                               // randomize within ties
                .Take(topK)
                .Select(kvp => kvp.Key)
                .ToList();

            int selectedKey = topKKeys[rng.Next(topKKeys.Count)];

            return _dictionary.TryGetDefinitionByKey(selectedKey) ?? throw new Exception();

        }

        public DictionaryDefinition GetInitialVocabEstimationDefinition()
        {
            // adaptive binning approach to get good coverage
            int chosenId;

            if (DefinitionFrequencyEngine.DefinitionZipfScores is null)
            {
                Log.Error("need to compute the definition Zipf scores");
                throw new Exception();
            }

            IReadOnlyDictionary<int, TestRecord> latest = _testRecords.LatestTestRecords();

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
                DictionaryDefinition? ret = GetFrequentDefinition(1);

                int cnt = 0;
                while (ret is not null && latest.ContainsKey(ret.DatabasePrimaryKey))
                {
                    ret = GetFrequentDefinition(1);
                    cnt++;
                    if (cnt > _minWordsTested)
                    {
                        Log.Warning("couldn't find a definition to test, even though we should have enough data");
                        ret = null;
                        break;
                    }
                }

                Log.Information("took {count} cycles to find a unseen word", cnt);

                if (ret is null)
                {
                    return GetRandomDefinition();
                }

                return ret;
            }

            chosenId = candidates[rng.Next(candidates.Count())].Key;

            return _dictionary.TryGetDefinitionByKey(chosenId) ?? throw new Exception();
        }
    }
}

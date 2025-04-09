using Infrastructure;
using Learning;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DataClasses;
using Config;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections;
using Microsoft.Extensions.Azure;
using Windows.ApplicationModel.Contacts.DataProvider;
using System.CodeDom;

namespace Linguine
{
    public record WordInContext(string StatementText, int WordStart, int Len);
    public record DefinitionForTesting(
        String Prompt, 
        String CorrectAnswer, 
        List<WordInContext> Contexts,
        DictionaryDefinition Parent,
        String? SoundFileName);

    public partial class MainModel
    {
        // TODO - this should be a lot more fleshed out
        // but for now we'll just a basic add and export functionality

        // TODO - this is getting a handful, need to sort out the lazy loading stuff
        // currently I added a LoadServices method in the main loading sequence
        private DefinitionLearningService? _defLearningService = null;
        private TestRecords? _testRecords;
        private int MaxContextExamples = 5;

        private Random _rng = new Random(Environment.TickCount);

        public int DistintWordsTested => _testRecords?.NumberDistinctDefinitionsTested() ?? 0;

        public DefinitionLearningService? DefLearning => _defLearningService;

        public bool NeedToImportADictionary { get; set; } = true;


        private DefinitionLearningService DefLearningService
        {
            get
            {
                if (_defLearningService is null)
                {
                    InitialiseDefinitionLearningService();
                }

                if (_defLearningService is null)
                {
                    Log.Error("tried and failed to load definition learning service");
                    throw new Exception();
                }

                return _defLearningService;
            }
        }


        internal bool EnoughDataForWordFrequencies()
        {
            if (NeedToImportADictionary) { return false; }

            return DefLearningService.EnoughDataForWordFrequencies();
        }


        internal bool AnyDataForWordFrequencies()
        {
            if (NeedToImportADictionary) { return false; }

            return DefLearningService.AnyDataForWordFrequencies();
        }

        internal bool NeedToBurnInVocabularyData()
        {
            if (NeedToImportADictionary) { return true; }

            return DefLearningService.NeedToBurnInVocabularyData();
        }


        private void InitialiseDefinitionLearningService()
        {
            if (HasManagers == false)
            {
                LoadManagers();
            }

            List<String> dictionaries = ExternalDictionaryManager.AvailableDictionaries();

            if (dictionaries.Count > 1)
            {
                throw new NotImplementedException("need to implement multiple dictionaries");
            }

            if (dictionaries.Count == 0)
            {
                NeedToImportADictionary = true;
                return;
            }

            NeedToImportADictionary = false; // TODO - random flags not great

            ExternalDictionary? dictionary = ExternalDictionaryManager.GetDictionary(dictionaries.First());

            if (dictionary is null)
            {
                Log.Error("failed to load the dictionary");
                return;
            }

            TestRecords testRecords = new TestRecords(dictionary, _linguineDbContextFactory);
            _testRecords = testRecords;

            using var context = _linguineDbContextFactory.CreateDbContext();
            DefinitionFrequencyEngine.UpdateDefinitionFrequencies(context);

            // TODO - carefully look into what order all the models are created
            // TODO - lazy loading
            // TODO - handle when there are no test records!

            InitVocabularyModel(); // def learning service needs the vocab model

            if (VocabModel is null)
            {
                throw new Exception("failed to init vocab model");
            }

            _defLearningService = new DefinitionLearningService(
                dictionary, testRecords, ParsedDictionaryDefinitionManager, StatementManager, VocabModel);

        }

        private WordInContext? AsWordInContext(DictionaryDefinition def, Statement context)
        {
            Tuple<int, int> startLen = StatementHelper.GetStartLenOfDefinition(context, def);

            if (startLen is null) { return null; }

            WordInContext ret = new WordInContext(
                context.StatementText,
                startLen.Item1,
                startLen.Item2
            );
            return ret;
        }

        public DefinitionForTesting AsDefinitionForTesting(DictionaryDefinition def)
        {
            List<Statement> uses = _statementManager.GetNStatementsFor(def, MaxContextExamples);

            List<WordInContext> contexts = uses.Where(use => use is not null).Select(use => AsWordInContext(def, use)).ToList();

            contexts = contexts.Where(c => c is not null).ToList();

            String? defText = null;

            List<VocalisedDefinitionFile> soundFiles = DefinitionVocalisationManager.GetAudioFilesForDefinition(def.DatabasePrimaryKey);

            String? soundFile = null;

            if (soundFiles.Count != 0)
            {
                soundFile = soundFiles[_rng.Next(soundFiles.Count)].FileName;

                soundFile = Path.Combine(ConfigManager.Config.AudioStoreDirectory, soundFile);
            }

            if (soundFiles.Count < Enum.GetNames(typeof(Voice)).Length)
            {
                var voices = Enum.GetValues(typeof(Voice));

                HashSet<Voice> heard = soundFiles.Select(
                    sf => sf.Voice).ToHashSet();

                List<Voice> unheard = new List<Voice>();

                foreach (Voice voice in voices)
                {
                    if (heard.Contains(voice))
                    {
                        continue;
                    } 
                    else
                    {
                        unheard.Add(voice);
                    }
                }

                if (unheard.Count != 0)
                {
                    Voice newVoice = unheard[_rng.Next(unheard.Count)];

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            await VocaliseDefinition(def, newVoice);

                            // uncomment to tidy - TODO - add a button somewhere??
                            //await DefinitionVocalisationManager.CleanupMissingFilesAsync();

                            // go off in the background and get another sound file
                            // since we probably care about this definition
                        }
                        catch (Exception ex)
                        {
                            Log.Error($"Exception in background vocalisation task for definition {def.Word}: {ex.Message}");
                        }
                    });
                }
                else
                {
                    Log.Information("something weird happened");
                }
            }

            if (ConfigManager.Config.LearningForeignLanguage())
            {
                // TODO - should these be implicit in the parsed dictionary definition manager?
                ParsedDictionaryDefinition? pdef = ParsedDictionaryDefinitionManager.GetParsedDictionaryDefinition(
                    def, ConfigManager.Config.GetLearnerLevel(), ConfigManager.Config.Languages.NativeLanguage);

                // TODO - once have multiple parsing levels - use a fallback to neares parsed level if the correct one is not available
                // but then spin off a thread to go do the parsing at the right level for future uses

                if (pdef is not null)
                {
                    defText = pdef.ParsedDefinition;
                } 
            } 

            if (defText is null)
            {
                defText = def.Definition;
            }

            if (def.RomanisedPronuncation is not null)
            {
                defText = def.RomanisedPronuncation + '\n' + defText;
            }

            return new DefinitionForTesting(def.Word, defText, contexts, def, soundFile);
        }


        public DefinitionForTesting GetHighLearningDefinition()
        {
            int defId = _defLearningService.GetHighLearningDefinitionID();

            ExternalDictionary? dictionary = ExternalDictionaryManager.GetFirstDictionary();

            DictionaryDefinition toTest = dictionary.TryGetDefinitionByKey(defId) ?? throw new Exception();

            if (toTest is null)
            {
                throw new Exception();
            }

            DefinitionForTesting forTesting = AsDefinitionForTesting(toTest);

            if (forTesting.Contexts.Count == 0)
            {
                // edge case where user knows every word every seen
                return AsDefinitionForTesting(DefLearningService.GetFrequentDefinition(1));
            }
            else
            {
                return forTesting;
            }
        }

        public DefinitionForTesting GetRandomDefinition()
        {
            DictionaryDefinition toTest = DefLearningService.GetFrequentDefinition();

            return AsDefinitionForTesting(toTest);
        }

        internal DefinitionForTesting GetHighInformationDefinition()
        {
            DictionaryDefinition toTest = DefLearningService.GetInitialVocabEstimationDefinition();

            return AsDefinitionForTesting(toTest);

        }

        private List<TestRecord>? currentSession;

        internal void RecordTest(DefinitionForTesting definitionForTesting, 
            DateTime posed, DateTime answered, DateTime finished,
            bool correct)
        {
            if (_testRecords is null)   
            {
                Log.Warning("test records should not be null, initialising");
                InitialiseDefinitionLearningService();
            }
            if (_testRecords is null)
            {
                Log.Fatal("failed to innitialise test records");
                throw new Exception();
            }

            TestRecord added = _testRecords.AddRecord(definitionForTesting.Parent, posed, answered, finished, correct);

            _defLearningService.Inform(added);
            
        }

        public List<DictionaryDefinition> LearnerList { get; private set; } = new List<DictionaryDefinition>();

        public void AddLearnerListItem(DictionaryDefinition definition)
        {
            LearnerList.Add(definition);
        }

        public bool ExportLearnerListToCSV(string savePath)
        {

            using (StreamWriter writer = new StreamWriter(savePath))
            {
                // writer.WriteLine("Word,Definition"); // import better into Anki without this

                foreach (DictionaryDefinition def in LearnerList)
                {
                    writer.WriteLine($"{def.Word},{def.Definition}");
                }
            }

            LearnerList = new List<DictionaryDefinition>();

            return true;
        }

        public VocabularyModel? VocabModel {get; set;}

        internal VocabularyModel ComputeVocabularyModel()
        {
            if (DefinitionFrequencyEngine.DefinitionZipfScores is null)
            {
                using var context = _linguineDbContextFactory.CreateDbContext();
                DefinitionFrequencyEngine.UpdateDefinitionFrequencies(context);
            }

            IReadOnlyDictionary<int, int>? freqs = DefinitionFrequencyEngine.DefinitionFrequencies;
            IReadOnlyDictionary<int, double>? zipfs = DefinitionFrequencyEngine.DefinitionZipfScores;

            if (freqs is null || zipfs is null)
            {
                throw new Exception("couldn't generate Zipf scores!");
            }

            IReadOnlyDictionary<int, List<TestRecord>>? testRecords = _testRecords?.Last5TestRecords();

            if (testRecords is null)
            {
                throw new Exception("couldn't obtain latest test records!");
            }

            // Todo - use strategist for this down the line

            IReadOnlyDictionary<int, double> pKnownNow = testRecords.ToDictionary(
                kvp => kvp.Key,
                kvp => kvp.Value.Average(record => record.Correct ? 1.0 : 0.0)
            );


            IReadOnlyDictionary<int, bool> firstWasCorrect = _testRecords?.FirstWasCorrect();

            if (firstWasCorrect is null)
            {
                throw new Exception("couldn't obtain first was correct data");
            }

            VocabularyModel ret = new VocabularyModel(
                freqs,
                firstWasCorrect,
                zipfs,
                DefinitionFrequencyEngine.ZipfHi,
                DefinitionFrequencyEngine.ZipfLo);

            ret.ComputePKnownWithError();

            return ret;
        }


        internal void InitVocabularyModel()
        {
            VocabModel = ComputeVocabularyModel();
        }

        public void UpdateVocabularyModel()
        {
            using var context = _linguineDbContextFactory.CreateDbContext();
            DefinitionFrequencyEngine.UpdateDefinitionFrequencies(context);

            VocabModel = ComputeVocabularyModel();
        }
    }
}

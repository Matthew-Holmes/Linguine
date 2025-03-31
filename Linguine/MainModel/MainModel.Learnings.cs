using Infrastructure;
using Infrastructure.DataClasses;
using Learning;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    public record DefinitionForTesting(String prompt, String correctAnswer, DictionaryDefinition parent);

    public partial class MainModel
    {
        // TODO - this should be a lot more fleshed out
        // but for now we'll just a basic add and export functionality

        // TODO - this is getting a handful, need to sort out the lazy loading stuff
        // currently I added a LoadServices method in the main loading sequence
        private DefinitionLearningService? _defLearningService = null;
        private TestRecords? _testRecords;

        public int DistintWordsTested => _testRecords?.DistinctDefinitionsTested() ?? 0;

        public DefinitionLearningService? DefLearning => _defLearningService;

        public bool NeedToImportADictionary { get; private set; } = true;


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

            _defLearningService = new DefinitionLearningService(
                dictionary, testRecords, ParsedDictionaryDefinitionManager, StatementManager);

        }


        public DefinitionForTesting GetRandomDefinitionForTesting()
        {
            DictionaryDefinition toTest = DefLearningService.GetFrequentDefinition();

            return new DefinitionForTesting(toTest.Word, toTest.Definition, toTest);
        }

        internal DefinitionForTesting GetHighInformationDefinition()
        {
            DictionaryDefinition toTest = DefLearningService.GetInitialVocabEstimationDefinition();

            return new DefinitionForTesting(toTest.Word, toTest.Definition, toTest);

        }

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

            _testRecords.AddRecord(definitionForTesting.parent, posed, answered, finished, correct);
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


    }
}

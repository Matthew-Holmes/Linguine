using Infrastructure;
using Infrastructure.DataClasses;
using Learning;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    public record DefinitionForTesting(String prompt, String correctAnswer);

    public partial class MainModel
    {
        // TODO - this should be a lot more fleshed out
        // but for now we'll just a basic add and export functionality

        private DefinitionLearningService? _defLearningService = null;
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
            return DefLearningService.EnoughDataForWordFrequencies();
        }



        internal bool NeedToBurnInVocabularyData()
        {
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
                Log.Error("tried to initialise definition service, but no definitions!");
                return;
            }

            ExternalDictionary? dictionary = ExternalDictionaryManager.GetDictionary(dictionaries.First());

            if (dictionary is null)
            {
                Log.Error("failed to load the dictionary");
                return;
            }

            TestRecords testRecords = new TestRecords(dictionary, _linguineDbContextFactory);

            using var context = _linguineDbContextFactory.CreateDbContext();
            DefinitionFrequencyEngine.UpdateDefinitionFrequencies(context);

            _defLearningService = new DefinitionLearningService(
                dictionary, testRecords, ParsedDictionaryDefinitionManager, StatementManager);

        }


        public DefinitionForTesting GetRandomDefinitionForTesting()
        {
            DictionaryDefinition toTest = DefLearningService.GetFrequentDefinition();

            return new DefinitionForTesting(toTest.Word, toTest.Definition);
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

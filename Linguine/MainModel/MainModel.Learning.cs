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
using System.Threading;

namespace Linguine
{
    public partial class MainModel
    {
        private IDefinitionLearningService? _defLearningService = null;

        private int MaxContextExamples = 5;

        private Random _rng = new Random(Environment.TickCount);


        // TODO - get this from a service manager

        private IDefinitionLearningService DefLearningService
        {
            get
            {
                // TODO - how to make this wait for the service to be loaded??
                // only allow the learn button once it is ready
                // how to handle the vocab model??

                if (_defLearningService is null)
                {
                    Log.Error("tried and failed to load definition learning service");
                    throw new Exception();
                }

                return _defLearningService;
            }
        }

        internal void StartDefinitionLearningService()
        {
            if (HasManagers == false)
            {
                throw new Exception("managers not loaded yet");
            }

            if (DictionaryDefinitionManager is null)
            {
                NeedToImportADictionary = true;
                return;
            }

            NeedToImportADictionary = false; // TODO - random flags not great

            // TODO - refactor the freqency engine into the learning service startup

            if (DefinitionFrequencyEngine.FrequencyData is null)
            {
                using var context = _linguineDbContextFactory.CreateDbContext();
                DefinitionFrequencyEngine.UpdateDefinitionFrequencies(context);
            }

            FrequencyData? freqData = DefinitionFrequencyEngine.FrequencyData;

            if (freqData is null)
            {
                throw new Exception("couldn't generate frequency data");
            }

            if (DictionaryDefinitionManager is null)
            {
                throw new Exception("trying to access the dictionary before it is available");
            }

            TestRecordsManager trm = new TestRecordsManager(_linguineReadonlyDbContextFactory); // TODO - homogonise this!!

            List<TestRecord>? allRecords = trm.AllRecordsTimeSorted();

            _defLearningService = DefinitionLearningServiceFactory.BuildDLS(freqData, allRecords, ConfigManager.CancelRunningTasksSource);
        }



        #region learner list for csv export

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

        #endregion

    }
}

using Config;
using DataClasses;
using Infrastructure;
using Learning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Linguine
{
    public enum ServiceState
    {
        NotStarted,
        Loading,
        Loaded,
        Failed,
        ShuttingDown,
        Stopped
    }


    public class Services
    {
        public IDefinitionLearningService? DefLearning      { get; set;  } = null;
        public ServiceState                DefLearningState { get; init; } = ServiceState.NotStarted;
    }

    partial class ServiceManager
    {
        // services
        // lazy/background loading 
        // service state - notStarted, loading, loaded, failed, shuttingDown, stopped
        // nullable - if null then the model can query the state?
        // track when a distribution is out of date - new data loaded

        public Services Services { get; init; } = new Services();


        public void InitialiseServices()
        {
            //IDefinitionLearningService dl = DefinitionLearningServiceFactory.BuildDLS();
        }

        internal void StartDefinitionLearningService()
        {
            if (ManagerState != DataManagersState.Initialised)
            {
                throw new Exception("managers not loaded yet");
            }

            if (Services.DefLearningState is ServiceState.Loading)
            {
                return; // TODO - should we be waiting here??
            }

            if (Services.DefLearningState is not ServiceState.NotStarted)
            {
                throw new Exception("already started, should be using restart");
            }

            if (Engines.DefFrequenciesState is EngineState.NotYetStarted)
            {
                StartDefinitionFrequencyEngine();
            }

            while (Engines.DefFrequenciesState is EngineState.Building)
            {
                Thread.Sleep(5);
            }
            
            using var context = DBF.CreateDbContext();

            FrequencyData freqData = Engines.DefFrequencies!.ComputeFrequencyData(context);

            List<TestRecord>? allRecords = Managers!.TestRecords.AllRecordsTimeSorted();

            Services.DefLearning = DefinitionLearningServiceFactory.BuildDLS(freqData, allRecords, ConfigManager.CancelRunningTasksSource);
        }

    }
}

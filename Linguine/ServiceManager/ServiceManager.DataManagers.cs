using Infrastructure;
using Learning;
using LearningExtraction;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Notifications;

namespace Linguine
{
    public enum DataQuality
    {
        NeedManagers,
        NeedDictionary,
        NeedMoreTextProcessed,
        NeedMoreVocabTested,
        Good,
    }

    public enum DataManagersState
    {
        NoDatabaseYet,
        Initialising,
        Initialised,
    }

    public class Managers
    {
        public required DefinitionVocalisationManager     Vocalisations     { get; set; }
        public required DictionaryDefinitionManager       Definitions       { get; set; }
        public required ParsedDictionaryDefinitionManager ParsedDefinitions { get; set; }
        public required StatementManager                  Statements        { get; set; }
        public required TestRecordsManager                TestRecords       { get; set; }
        public required TextualMediaManager               TextualMedia      { get; set; }
        public required TextualMediaSessionManager        Sessions          { get; set; }
    }


    partial class ServiceManager
    {
        private int _minWordsProcessed = 300;
        private int _minWordsTested    = 50;

        // TODO - callbacks when these change??
        public DataManagersState ManagerState { get; private set; } = DataManagersState.NoDatabaseYet;
        public DataQuality       DataQuality  { get; private set; } = DataQuality.NeedManagers;  


        public Managers? Managers { get; private set; } = null;


        private void InitialiseManagers(LinguineReadonlyDbContextFactory dbf)
        {
            ManagerState = DataManagersState.Initialising;

            Managers = new Managers
            {
                Vocalisations     = new DefinitionVocalisationManager(dbf),
                Definitions       = new DictionaryDefinitionManager(dbf),
                ParsedDefinitions = new ParsedDictionaryDefinitionManager(dbf),
                Statements        = new StatementManager(dbf),
                TestRecords       = new TestRecordsManager(dbf),
                TextualMedia      = new TextualMediaManager(dbf),
                Sessions          = new TextualMediaSessionManager(dbf),
            };

            InspectDataQuality();

            ManagerState = DataManagersState.Initialised;
        }

        internal void InspectDataQuality()
        {
            if (Managers is null) { return; }

            if (!Managers.Definitions.AnyDefinitions())
            {
                DataQuality = DataQuality.NeedDictionary;
                return;
            }

            if (Managers.Statements.UniqueDefinitionsObserved() < _minWordsProcessed)
            {
                DataQuality = DataQuality.NeedMoreTextProcessed;
                return;
            }

            if (Managers.TestRecords.UniqueDefinitionsTested() < _minWordsTested)
            {
                DataQuality = DataQuality.NeedMoreVocabTested;
                return;
            }

            DataQuality = DataQuality.Good;
        }
    }
}

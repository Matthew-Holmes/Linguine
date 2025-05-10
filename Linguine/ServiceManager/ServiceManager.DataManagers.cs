using Infrastructure;
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
        Disposing,
        Disposed,
    }
    // TODO - do we need the dispose stuff, or handle that with IDisposable
    // TODO - what to do about readonly handles etc??

    public class Managers
    {
        public required DefinitionVocalisationManager     Vocalisations     { get; set; }
        public required DictionaryDefinitionManager       Definitions       { get; set; }
        public required ParsedDictionaryDefinitionManager ParsedDefinitions { get; set; }
        public required StatementManager                  Statements        { get; set; }
        public required TestRecordsManager                TestRecords       { get; set; }
        public required TextualMediaManager               TextualMedia      { get; set; }
        public required TextualMediaSessionManager        SessionManager    { get; set; }
    }


    partial class ServiceManager
    {
        // TODO - callbacks when these change??
        public DataManagersState ManagerState { get; private set; }
        public DataQuality       DataQuality  { get; private set; }


        // would a union type make sense here??
        public Managers? Managers           { get; private set; }
        public Managers? ReadOnlyManangers  { get; private set; }

        public void InitialiseManager()
        {
            throw new NotImplementedException();
        }


    }
}

using Config;
using Infrastructure;
using Learning;
using LearningExtraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Serilog;

namespace Linguine
{

    public enum EngineState
    {
        NotYetStarted,
        Building,
        Built,
        Failed,
        //Stale, // do we want something like this??
    }

    // single task engines for achieving specific data processing/generation

    public class Engines
    {
        public IDefinitionFrequencyEngine? DefFrequencies { get; set; } = null;
        public EngineState DefFrequenciesState { get; set; } = EngineState.NotYetStarted;

        public ICanAnalyseText? TextAnalyser { get; set; }
        public EngineState TextAnalyserState { get; set; } = EngineState.NotYetStarted;
        
        public ICanParseDefinitions? DefinitionParser { get; set; }
        public EngineState DefinitionParserState { get; set; } = EngineState.NotYetStarted;

        public ICanPronounce? Pronouncer { get; set; }
        public EngineState PronouncerState { get; set; } = EngineState.NotYetStarted;
        
        public ICanResolveDefinitions? DefinitionResolver { get; set; }
        public EngineState DefinitionResolverState { get; set; } = EngineState.NotYetStarted
    }

    partial class ServiceManager
    {
        public Engines Engines { get; init; } = new Engines();

        // TODO - enable JIT/Lazy startup?? or keep in a background thread

        public void InitialiseEngines()
        {
            StartDefinitionFrequencyEngine();
            StartTextAnalyser();
            StartParsingEngine();
            StartPronunciationEngine();
            StartDefinitionResolutionEngine();
        }

        public void StartDefinitionFrequencyEngine()
        {

            if (!CheckStartupState(Engines.DefFrequenciesState))
            {
                return;
            }

            Engines.DefFrequenciesState = EngineState.Building;

            Engines.DefFrequencies = DefinitionFrequencyEngineFactory.BuildDefinitionFrequencyEngine();

            Engines.DefFrequenciesState = EngineState.Built;
        }

        public void StartTextAnalyser()
        {
            if (!CheckStartupState(Engines.TextAnalyserState))
            {
                return;
            }

            Engines.TextAnalyserState = EngineState.Building;

            if (DataQuality == DataQuality.NeedDictionary)
            {
                Engines.TextAnalyserState = EngineState.Failed;
                Log.Error("can't start statement engine without a dictionary");
                return;
            }

            if (ManagerState != DataManagersState.Initialised)
            {
                InitialiseManagers(DBF); // TODO - this should be a dependancy??
            }

            Engines.TextAnalyser = StatementEngineFactory.BuildStatementEngine(Managers!.Definitions);

            Engines.TextAnalyserState = EngineState.Built;

        }

        private void StartParsingEngine()
        {
            if (!CheckStartupState(Engines.DefinitionParserState))
            {
                return;
            }

            Engines.DefFrequenciesState = EngineState.Building;

            Engines.DefinitionParser = DefinitionParserFactory.BuildParsingEngine();

            Engines.DefFrequenciesState = EngineState.Built;
        }

        private void StartPronunciationEngine()
        {
            if (!CheckStartupState(Engines.PronouncerState))
            {
                return;
            }

            Engines.PronouncerState = EngineState.Building;

            Engines.Pronouncer = DefinitionPronouncerFactory.BuildPronunciationEngine();

            Engines.PronouncerState = EngineState.Built;
        }

        private void StartDefinitionResolutionEngine()
        {
            if (!CheckStartupState(Engines.DefinitionResolverState))
            {
                return;
            }

            Engines.DefinitionResolverState = EngineState.Building;

            Engines.DefinitionResolver = InteractiveDefinitionResolutionEngineFactory.BuildDefinitionResolutionEngine();

            Engines.DefinitionResolverState = EngineState.Built;
        }

        private bool CheckStartupState(EngineState state)
        {
            if (state is EngineState.Building)
            {
                return false;
            }

            if (state is not EngineState.NotYetStarted)
            {
                throw new Exception("already started, should be using restart");
            }

            if (state is EngineState.NotYetStarted)
            {
                return true;
            }

            throw new NotImplementedException();
        }


        // TODO - cache the result from def frequency engine??
        // may not need both
        // Statement engine
        // Definition frequency engine
    }
}

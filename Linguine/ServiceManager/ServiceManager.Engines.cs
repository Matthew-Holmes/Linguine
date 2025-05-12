using Infrastructure;
using Learning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{

    public enum EngineState
    {
        NotYetStarted,
        Building,
        Built,
        //Stale, // do we want something like this??
    }

    // single task engines for achieving specific data processing/generation

    public class Engines
    {
        public IDefinitionFrequencyEngine? DefFrequencies { get; set; } = null;
        public EngineState DefFrequenciesState { get; set; } = EngineState.NotYetStarted;
    }

    partial class ServiceManager
    {
        // have the engines as properties??

        public Engines Engines { get; init; } = new Engines();

        public void StartDefinitionFrequencyEngine()
        {
            if (Engines.DefFrequenciesState is not EngineState.NotYetStarted)
            {
                throw new Exception("already started, should be using restart");
            }

            if (Engines.DefFrequenciesState is EngineState.Building)
            {
                return;
            }

            Engines.DefFrequenciesState = EngineState.Building;

            Engines.DefFrequencies = DefinitionFrequencyEngineFactory.BuildDefinitionFrequencyEngine();

            Engines.DefFrequenciesState = EngineState.Built;
        }


        // TODO - cache the result from def frequency engine??
        // may not need both
        // Statement engine
        // Definition frequency engine
    }
}

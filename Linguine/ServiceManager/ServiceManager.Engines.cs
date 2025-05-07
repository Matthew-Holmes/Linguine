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
        Stale,
        Disposing,
        Disposed,
    }


    partial class ServiceManager
    {
        // have the engines as properties??
    }
}

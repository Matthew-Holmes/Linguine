using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    internal enum ServiceState
    {
        NotStarted,
        Loading,
        Loaded,
        Failed,
        ShuttingDown,
        Stopped
    }


    partial class ServiceManager
    {
        // services
        // lazy/background loading 
        // service state - notStarted, loading, loaded, failed, shuttingDown, stopped
        // nullable - if null then the model can query the state?
        // track when a distribution is out of date - new data loaded
    }
}

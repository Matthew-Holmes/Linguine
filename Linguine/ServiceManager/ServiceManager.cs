using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    internal partial class ServiceManager
    {
        // engines
        // services
        // data access

        // services
        // lazy/background loading 
        // service state - notStarted, loading, loaded, failed, shuttingDown, stopped
        // nullable - if null then the model can query the state?
        // track when a distribution is out of date - new data loaded
        
        // data
        // manage caching??
        // how to avoid layer hell?
        // should we put the context factory here? 

        // engines
        // read only database handles??
        // main model orchestrates engine with data saving
        // why does learning extraction need Infra
        // can we make its functions only require readonly handles


        // Dispose
        // should clean up all resources

        private LinguineReadonlyDbContextFactory DBF { get; init; }

        public ServiceManager(LinguineReadonlyDbContextFactory dbf)
        {
            DBF = dbf;
        }

        public void Initialise()
        {
            InitialiseManagers(DBF);
        }

        // TODO - readonly context factory that managers can have instance of
        // but changing data methods require a context that only the MainModel can generate??
        // TODO - dispose that cascade disposes all handles generated from the dbf??

    }
}

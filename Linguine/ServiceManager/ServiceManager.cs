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

        private LinguineReadonlyDbContextFactory DBF { get; init; }

        public ServiceManager(LinguineReadonlyDbContextFactory dbf)
        {
            DBF = dbf;
        }

        public void Initialise()
        {
            // must retain this order
            InitialiseManagers(DBF);

            if (DataQuality is not DataQuality.NeedDictionary)
            {
                // TODO - fix this, some services may not need a dictionary
                InitialiseEngines();
                InitialiseServices();
            }
        }
    }
}

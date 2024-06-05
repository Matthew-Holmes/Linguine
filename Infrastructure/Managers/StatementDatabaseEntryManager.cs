using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class StatementDatabaseEntryManager : ManagerBase
    {
        public StatementDatabaseEntryManager(LinguineDataHandler db) : base(db)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{ 
    public class ManagerBase
    {
        protected LinguineDataHandler _db;

        public ManagerBase(LinguineDataHandler db)
        {
            _db = db;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{ 
    public class ManagerBase
    {
        protected LinguineDbContextFactory _dbf;

        public ManagerBase(LinguineDbContextFactory dbf)
        {
            _dbf = dbf;
        }
    }
}

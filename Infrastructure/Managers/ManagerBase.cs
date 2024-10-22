using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{ 
    public class ManagerBase
    {
        protected readonly String _connectionString;

        public ManagerBase(String connectionString)
        {
            _connectionString = connectionString;
        }

        protected LinguineContext Linguine()
        {
            return new LinguineContext(_connectionString);
        }
    }
}

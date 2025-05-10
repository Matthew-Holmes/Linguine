using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class LinguineReadonlyDbContextFactory
    {
        private LinguineDbContextFactory _dbf;

        public LinguineReadonlyDbContextFactory(LinguineDbContextFactory dbf)
        {
            _dbf = dbf;
        }

        public LingineReadonlyDbContext CreateDbContext()
        {
            LinguineDbContext core = _dbf.CreateDbContext();

            return new LingineReadonlyDbContext(core);
        }
    }
}

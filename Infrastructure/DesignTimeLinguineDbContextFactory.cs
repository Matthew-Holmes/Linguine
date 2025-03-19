using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    class DesignTimeLinguineDbContextFactory : IDesignTimeDbContextFactory<LinguineDbContext>
    {
        public LinguineDbContext CreateDbContext(string[] args)
        {
            string connectionString = args.Length > 0 ? args[0] : throw new Exception("no connection string");

            return new LinguineDbContext(connectionString);
        }
    }
}

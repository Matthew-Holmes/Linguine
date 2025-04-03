using Microsoft.EntityFrameworkCore.Design;

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

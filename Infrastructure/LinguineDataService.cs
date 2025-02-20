using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    // TODO - is this necessary??
    public class LinguineDataService
    {
        private readonly IDbContextFactory<LinguineDbContext> _contextFactory;
        private static readonly SemaphoreSlim _dbLock = new SemaphoreSlim(1, 1);

        public LinguineDataService(IDbContextFactory<LinguineDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task Save()
        {
            await _dbLock.WaitAsync();
            try
            {
                using var context = _contextFactory.CreateDbContext();
                // Perform database operations safely
                await context.SaveChangesAsync();
            }
            finally
            {
                _dbLock.Release();
            }
        }
    }
}

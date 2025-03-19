using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore.Design;

namespace Infrastructure
{
    public class LinguineDbContextFactory 
    {
        private readonly String _connString;

        public LinguineDbContextFactory(String connString)
        {
            _connString = connString;
        }

        public LinguineDbContext CreateDbContext()
        {
            Log.Verbose("Created Context. StackTrace: {StackTrace}", GetFilteredStackTrace());
            return new LinguineDbContext(_connString);
        }

        private string GetFilteredStackTrace()
        {
            StackTrace stackTrace = new StackTrace(true); // true = include file info
            return string.Join("\n", stackTrace.GetFrames()
                .Where(f => f.GetMethod().DeclaringType != null &&
                            f.GetMethod().DeclaringType.Namespace != null &&
                            !f.GetMethod().DeclaringType.Namespace.StartsWith("System") &&
                            !f.GetMethod().DeclaringType.Namespace.StartsWith("MS"))
                .Select(f => $"{f.GetMethod().DeclaringType}.{f.GetMethod().Name} ({f.GetFileName()}:{f.GetFileLineNumber()})"));
        }
    }
}

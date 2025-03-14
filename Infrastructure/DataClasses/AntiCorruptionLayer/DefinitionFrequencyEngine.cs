using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataClasses    
{
    public static class DefinitionFrequencyEngine
    {
        public static IReadOnlyDictionary<int, int>? DefinitionFrequencies { get; private set; } 

        public static void UpdateDefinitionFrequencies(LinguineDbContext context)
        {
            // TODO - when this gets slow, we should cache them per textual media
            // then only update the ones that have changed (timestamps/change tracker)
            var frequencyTable = context.StatementDefinitions
                .GroupBy(node => node.DefinitionKey)
                .Select(group => new { DefinitionKey = group.Key, Count = group.Count() })
                .ToDictionary(x => x.DefinitionKey, x => x.Count);

            DefinitionFrequencies = frequencyTable.AsReadOnly();
        }

    }
}

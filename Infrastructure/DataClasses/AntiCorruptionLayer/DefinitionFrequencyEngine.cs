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

            //SaveDefinitionFrequenciesToCsv(context, "freqs.csv"); // TODO - remove when finished analysis!

            if (!context.StatementDefinitions.Any())
            {
                DefinitionFrequencies = new Dictionary<int, int>().AsReadOnly();
            }

            // include zero counted words in the frequency dictionary too

            var frequencyTable = context.DictionaryDefinitions
                .Select(node => new { DefinitionKey = node.DatabasePrimaryKey, Count = 0 })
                .ToDictionary(x=> x.DefinitionKey, x=>x.Count);

            var counts = context.StatementDefinitions
                .GroupBy(node => node.DefinitionKey)
                .Select(group => new { DefinitionKey = group.Key, Count = group.Count() })
                .ToDictionary(x => x.DefinitionKey, x => x.Count);

            foreach (KeyValuePair<int, int> item in counts)
            {
                frequencyTable[item.Key] = item.Value;
            }

            DefinitionFrequencies = frequencyTable.AsReadOnly();
        }

        public static void SaveDefinitionFrequenciesToCsv(LinguineDbContext context, string filePath)
        {
            var frequencyTable = context.StatementDefinitions
                .Include(s => s.DictionaryDefinition) // Eager load definitions
                .AsEnumerable() // Switch to in-memory processing
                .Where(s => s.DictionaryDefinition != null) // Ensure we have valid definitions
                .GroupBy(s => s.DefinitionKey)
                .Select(g => new
                {
                    Word = g.First().DictionaryDefinition!.Word, // Get the word
                    Frequency = g.Count(),
                    DefinitionText = g.First().DictionaryDefinition!.Definition
                })
                .OrderByDescending(x => x.Frequency)
                .ToList();

            using (var writer = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                writer.WriteLine("Word,Frequency,Definition");

                foreach (var entry in frequencyTable)
                {
                    string line = $"{EscapeCsv(entry.Word)},{entry.Frequency},{EscapeCsv(entry.DefinitionText)}";
                    writer.WriteLine(line);
                }
            }
        }

        private static string EscapeCsv(string input)
        {
            if (input.Contains(",") || input.Contains("\"") || input.Contains("\n"))
            {
                return $"\"{input.Replace("\"", "\"\"")}\"";
            }
            return input;
        }
    }

}

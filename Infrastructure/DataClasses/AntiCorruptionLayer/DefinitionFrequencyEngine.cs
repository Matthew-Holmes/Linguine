using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;

namespace Infrastructure.DataClasses    
{
    using DefinitionId       = Int32;
    using Frequency          = Int32;
    using FrequencyMap       = IReadOnlyDictionary<int, int>;
    using FrequencyBucketMap = IReadOnlyDictionary<int, HashSet<int>>;
    using ZipfMap            = IReadOnlyDictionary<int, double>;

    public static class DefinitionFrequencyEngine
    {
        public static FrequencyMap?       DefinitionFrequencies       { get; private set; }
        public static FrequencyBucketMap? SortedDefinitionFrequencies { get; private set; }
        public static ZipfMap?            DefinitionZipfScores        { get; private set; }
        public static double              ZipfLo                      { get; private set; }
        public static double              ZipfHi                      { get; private set; }

        public static void UpdateDefinitionFrequencies(LinguineDbContext context)
        {
            // basic frequencies

            if (!context.StatementDefinitions.Any())
            {
                DefinitionFrequencies = new Dictionary<DefinitionId, Frequency>().AsReadOnly();
                SortedDefinitionFrequencies = new Dictionary<Frequency, HashSet<DefinitionId>>().AsReadOnly();
                return;
            }

            var frequencyTable = context.DictionaryDefinitions
                .Select(node => new { DefinitionKey = node.DatabasePrimaryKey, Count = 0 })
                .ToDictionary(x => x.DefinitionKey, x => x.Count);

            var counts = context.StatementDefinitions
                .GroupBy(node => node.DefinitionKey)
                .Select(group => new { DefinitionKey = group.Key, Count = group.Count() })
                .ToDictionary(x => x.DefinitionKey, x => x.Count);

            foreach (KeyValuePair<DefinitionId, Frequency> item in counts)
            {
                frequencyTable[item.Key] = item.Value;
            }

            DefinitionFrequencies = frequencyTable.AsReadOnly();

            // sorted and binned frequencies

            var sortedFrequencies = new Dictionary<Frequency, HashSet<DefinitionId>>();

            foreach (var (definitionId, freq) in frequencyTable)
            {
                if (!sortedFrequencies.TryGetValue(freq, out var idSet))
                {
                    idSet = new HashSet<DefinitionId>();
                    sortedFrequencies[freq] = idSet;
                }

                idSet.Add(definitionId);
            }

            SortedDefinitionFrequencies = sortedFrequencies
                .OrderByDescending(kvp => kvp.Key)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value)
                .AsReadOnly();

            // Zipf scores

            double totalFrequency = frequencyTable.Values.Sum();

            var zipfScores = new Dictionary<DefinitionId, double>();
            if (totalFrequency == 0)
            {
                DefinitionZipfScores = frequencyTable.Keys.ToDictionary(id => id, _ => 0.0).AsReadOnly();
                Log.Error("total frequency zero when computing Zipf scores!");
                return;
            }

            // TODO - as outlined in notebooks, this falls victim to survivorship bias
            // we should account for this as described
            ZipfLo = double.MaxValue;
            ZipfHi = double.MinValue;

            foreach (var (definitionId, frequency) in frequencyTable)
            {
                double freqPerBillion;

                if (frequency == 0)
                {
                    freqPerBillion = 1.0; // dummy value keep it low score so we don't
                                          // inundate the user with random definitions
                }

                freqPerBillion = (frequency / totalFrequency) * 1_000_000_000.0;
                double zipf = Math.Log10(freqPerBillion) + 3;
                zipfScores[definitionId] = zipf;

                // track range we are confident in
                ZipfHi = Math.Max(ZipfHi, zipf);
                ZipfLo = Math.Min(ZipfLo, zipf);
            }

            DefinitionZipfScores = zipfScores.AsReadOnly();
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

using Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningStore
{
    public class ExternalDictionaryCSVParser
    {
        public static String ParseDictionaryFromCSVToSQLiteAndSave(
            String csvFileLocation,
            Config config,
            LanguageCode lc,
            String name)
        {
            var records = ParseCsv(csvFileLocation);

            if (records.Count == 0)
            {
                throw new DataException("record parsing failed, are you sure there are records present?");
            }

            // Construct the path to the database file, creating directory if it doesn't exist
            string dir = Path.Combine(config.FileStoreLocation, config.DictionariesDirectory, lc.ToString());
            System.IO.Directory.CreateDirectory(dir);

            string dbFilePath = Path.Combine(dir, name + ".db");

            // Create the connection string for SQLite
            string connectionString = $"Data Source={dbFilePath};";

            using ExternalDictionaryContext newContext = new ExternalDictionaryContext(connectionString);

            newContext.Database.EnsureCreated();

            foreach (var record in records)
            {
                newContext.DictionaryDefinitions.Add(record);
            }

            newContext.SaveChanges();

            return connectionString;
        }

        private static List<DictionaryDefinition> ParseCsv(string filePath)
        {
            var definitions = new List<DictionaryDefinition>();

            using (var reader = new StreamReader(filePath, Encoding.Unicode))
            {
                // Skip the first line (headers)
                reader.ReadLine();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    var parts = line.Split(new[] { ',' }, 3); // Split only into three parts
                    if (parts.Length == 3)
                    {
                        var definition = new DictionaryDefinition
                        {
                            ID = int.Parse(parts[0]),
                            Word = parts[1],
                            Definition = parts[2]
                        };

                        definitions.Add(definition);
                    }
                }
            }
            return definitions;
        }

    }
}

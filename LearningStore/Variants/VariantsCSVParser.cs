using Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningStore
{
    internal class VariantsCSVParser
    {
        public static String ParseVariantsFromCSVToSQLiteAndSave(
            String csvFileLocation,
            LanguageCode lc,
            String name)
        {
            var records = ParseCSV(csvFileLocation);

            if (records.Count == 0)
            {
                throw new DataException("record parsing failed, are you sure there are records present?");
            }

            // Construct the path to the database file, creating directory if it doesn't exist
            string dir = Path.Combine(ConfigManager.FileStoreLocation, ConfigManager.VariantsDirectory, lc.ToString());
            System.IO.Directory.CreateDirectory(dir);

            string dbFilePath = Path.Combine(dir, name + ".db");

            // Create the connection string for SQLite
            string connectionString = $"Data Source={dbFilePath};";

            using VariantsContext newContext = new VariantsContext(connectionString);

            newContext.Database.EnsureCreated();

            foreach (var record in records)
            {
                newContext.Variants.Add(record);
            }

            newContext.SaveChanges();

            return connectionString;
        }

        public static List<VariantRoot> ParseCSV(String filePath)
        {
            var variants = new List<VariantRoot>();

            using (var reader = new StreamReader(filePath, Encoding.Unicode))
            {
                // Skip the first line (headers)
                reader.ReadLine();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        var parts = line.Split(new[] { ',' }, 2);

                        if (parts.Length == 2)
                        {
                            var variantRoot = new VariantRoot
                            {
                                Variant = parts[0],
                                Root = parts[1]
                            };
                            variants.Add(variantRoot);
                        }

                    } catch (Exception e)
                    {
                        throw new Exception($"Parsing the following line failed {line}");
                    }
                }
            }
            return variants;
        }
    }
}

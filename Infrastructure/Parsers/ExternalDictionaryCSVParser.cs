using Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    internal class ExternalDictionaryCSVParser
    {
        public static void ParseDictionaryFromCSVToSQLiteAndSave(
            ExternalDictionary target,
            String csvFileLocation,
            String source)
        {
            if (target.Source != source)
            {
                throw new Exception("Dictionary source doesn't match provided");
            }

            var records = ParseCsv(csvFileLocation, source);

            if (records.Count == 0)
            {
                throw new DataException("record parsing failed, are you sure there are records present?");
            }

            target.Add(records);
        }

        private static List<DictionaryDefinition> ParseCsv(String filePath, String source)
        {
            var definitions = new List<DictionaryDefinition>();

            using (var reader = new StreamReader(filePath, Encoding.Unicode))
            {
                // Skip the first line (headers)
                reader.ReadLine();

                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    try
                    {
                        var parts = line.Split(new[] { ',' }, 3); // Split only into three parts
                        if (parts.Length == 3)
                        {
                            var definition = new DictionaryDefinition
                            {
                                ID = int.Parse(parts[0]),
                                Word = parts[1],
                                Definition = parts[2],
                                Source = source
                            };

                            definitions.Add(definition);
                        }
                    } catch (Exception e)
                    {
                        throw new Exception($"Parsing the following line failed {line}");
                    }
                }
            }
            return definitions;
        }

    }
}

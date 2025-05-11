using System.Data;
using System.Text;
using DataClasses;

namespace Infrastructure
{
    internal class ExternalDictionaryCSVParser
    {
        public static void ParseDictionaryFromCSVToSQLiteAndSave(
            DictionaryDefinitionManager mngr,
            String csvFileLocation,
            LinguineDbContext context)
        {
            var records = ParseCsv(csvFileLocation);

            if (records.Count == 0)
            {
                throw new DataException("record parsing failed, are you sure there are records present?");
            }

            mngr.Add(records, context);
        }

        private static List<DictionaryDefinition> ParseCsv(String filePath)
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
                            };

                            definitions.Add(definition);
                        }
                    } catch (Exception _)
                    {
                        throw new Exception($"Parsing the following line failed {line}");
                    }
                }
            }
            return definitions;
        }

    }
}

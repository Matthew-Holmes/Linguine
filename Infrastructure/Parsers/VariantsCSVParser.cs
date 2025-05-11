using System.Data;
using System.Text;
using DataClasses;

namespace Infrastructure
{
    internal class VariantsCSVParser
    {
        public static void ParseVariantsFromCSVToSQLiteAndSave(
            VariantsManager mngr,
            String csvFileLocation,
            LinguineDbContext context)
        {
            var records = ParseCSV(csvFileLocation);

            if (records.Count == 0)
            {
                throw new DataException("record parsing failed, are you sure there are records present?");
            }

            mngr.Add(records, context);
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
                                Root = parts[1],
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

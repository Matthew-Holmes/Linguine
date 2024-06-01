using Infrastructure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    internal class VariantsCSVParser
    {
        public static void ParseVariantsFromCSVToSQLiteAndSave(
            Variants target,
            String csvFileLocation,
            String source)
        {
            var records = ParseCSV(csvFileLocation, source);

            if (records.Count == 0)
            {
                throw new DataException("record parsing failed, are you sure there are records present?");
            }

            target.Add(records);
        }

        public static List<VariantRoot> ParseCSV(String filePath, String source)
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
                                Source = source
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

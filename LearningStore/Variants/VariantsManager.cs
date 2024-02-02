using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningStore
{
    public static class VariantsManager
    {
        public static List<String> AvailableVariantsSources(LanguageCode lc)
        {
            if (ConfigManager.SavedVariantsNamesAndConnnectionStrings.ContainsKey(lc))
            {
                return ConfigManager.SavedVariantsNamesAndConnnectionStrings[lc].Select(t => t.Item1).ToList();
            }
            else
            {
                return new List<String>();
            }
        }

        public static Variants? GetVariantsSource(LanguageCode lc, String name)
        {
            if (!AvailableVariantsSources(lc).Contains(name))
            {
                return null;
            }

            String connectionString = ConfigManager.SavedVariantsNamesAndConnnectionStrings[lc].Where(t => t.Item1 == name).FirstOrDefault().Item2;

            return new Variants(lc, name, connectionString);
        }

        public static void AddNewVariantsSourceFromCSV(LanguageCode lc, String name, String csvFileLocation)
        {
            if (AvailableVariantsSources(lc).Contains(name))
            {
                throw new InvalidDataException("naming conflict identified, variants adding aborted");
            }

            String connectionString = VariantsCSVParser.ParseVariantsFromCSVToSQLiteAndSave(
                csvFileLocation, lc, name);

            UpdateConfigWithExistingVariantsDatabase(lc, name, connectionString);
        }

        private static void UpdateConfigWithExistingVariantsDatabase(LanguageCode lc, string name, string connectionString)
        {
            Variants variants = new Variants(lc, name, connectionString); // check it exists
            variants.Dispose();

            if (ConfigManager.SavedVariantsNamesAndConnnectionStrings.ContainsKey(lc))
            {
                if (ConfigManager.SavedVariantsNamesAndConnnectionStrings[lc].Any(t => t.Item1 == name))
                {
                    throw new Exception("Already an existing source of variants with this name");
                }
            }

            ConfigManager.AddVariantsDetails(lc, Tuple.Create(name, connectionString));
        }

        public static void VerifyIntegrity()
        {
            if (!VerifyNoDuplicateVariantSources())
            {
                throw new Exception("Duplicate variant sources found");
            }

            if (!VerifyVariantSourcesExist())
            {
                throw new Exception("Expected variant sources not found");
            }
        }

        private static bool VerifyVariantSourcesExist()
        {
            foreach (LanguageCode lc in ConfigManager.SavedVariantsNamesAndConnnectionStrings.Keys)
            {
                if (!VerifyVariantSourcesExist(lc))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool VerifyVariantSourcesExist(LanguageCode lc)
        {

            if (!ConfigManager.SavedVariantsNamesAndConnnectionStrings.ContainsKey(lc))
            {
                return true; // vacuous truth
            }

            foreach (Tuple<String, String> details in ConfigManager.SavedVariantsNamesAndConnnectionStrings[lc])
            {
                try
                {
                    Variants variants = new Variants(lc, details.Item1, details.Item2);
                    variants.Dispose();
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            return true;
        }

        private static bool VerifyNoDuplicateVariantSources()
        {
            foreach (LanguageCode lc in ConfigManager.SavedVariantsNamesAndConnnectionStrings.Keys)
            {
                List<String> names = AvailableVariantsSources(lc);
                if (names.Distinct().Count() != names.Count())
                {
                    return false;
                }
            }
            return true;
        }


    }
}

using CsvHelper;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LearningStore
{
    public static class ExternalDictionaryManager
    {
        public static List<String> AvailableDictionaries(Config config, LanguageCode lc)
        {
            if (config.SavedDictionariesNamesAndConnnectionStrings.ContainsKey(lc))
            {
                return config.SavedDictionariesNamesAndConnnectionStrings[lc].Select(t => t.Item1).ToList();
            }
            else
            {
                return new List<string>();
            }
        }

        public static ExternalDictionary? GetDictionary(Config config, LanguageCode lc, String name)
        {
            if (!AvailableDictionaries(config, lc).Contains(name))
            {
                return null;
            }

            String connectionString = config.SavedDictionariesNamesAndConnnectionStrings[lc].Where(t => t.Item1 == name).FirstOrDefault().Item2;

            return new ExternalDictionary(lc, name, connectionString);
        }

        public static void AddNewDictionaryFromCSV(Config config, LanguageCode lc, String name, String csvFileLocation)
        {
            if (AvailableDictionaries(config, lc).Contains(name))
            {
                throw new InvalidDataException("naming conflict identified, dictionary adding aborted");
            }

            String connectionString = ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(
                csvFileLocation, config, lc, name);

            UpdateConfigWithExistingDictionaryDatabase(config, lc, name, connectionString); 
        }

        public static void VerifyIntegrityWith(Config config)
        {
            if (!VerifyNoDuplicateDictionaries(config))
            {
                throw new Exception("Duplicate dictionaries found");
            }

            if (!VerifyExternalDictionariesExist(config))
            {
                throw new Exception("Expected dictionary not found");
            }
        }


        private static void UpdateConfigWithExistingDictionaryDatabase(Config config, LanguageCode lc, String name, String connectionString)
        {
            ExternalDictionary dict = new ExternalDictionary(lc, name, connectionString); // check that it exists
            dict.Dispose();

            if (config.SavedDictionariesNamesAndConnnectionStrings is null)
            {
                config.SavedDictionariesNamesAndConnnectionStrings = new Dictionary<LanguageCode, List<Tuple<string, string>>>();
            }

            if (!config.SavedDictionariesNamesAndConnnectionStrings.ContainsKey(lc))
            {
                config.SavedDictionariesNamesAndConnnectionStrings[lc] = new List<Tuple<string, string>>();
            }

            if (config.SavedDictionariesNamesAndConnnectionStrings[lc].Any(t => t.Item1 == name))
            {
                throw new Exception("Already an existing dictionary with this name");
            }

            config.SavedDictionariesNamesAndConnnectionStrings[lc].Add(Tuple.Create(name, connectionString));

            ConfigManager.UpdateConfig(config);
        }

        private static bool VerifyNoDuplicateDictionaries(Config config)
        {
            foreach (LanguageCode lc in config.SavedDictionariesNamesAndConnnectionStrings.Keys)
            {
                List<String> names = AvailableDictionaries(config, lc);
                if (names.Distinct().Count() != names.Count())
                {
                    return false;
                }
            }
            return true;
        }

        private static bool VerifyExternalDictionariesExist(Config config)
        {
            // checks that all the dictionaries outlined in config do exist
            foreach (LanguageCode lc in config.SavedDictionariesNamesAndConnnectionStrings.Keys)
            {
                if (!VerifyExternalDictionariesExist(config, lc))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool VerifyExternalDictionariesExist(Config config, LanguageCode lc)
        {
            // checks that all the dictionaries outlined in config do exist for a specific language

            if (!config.SavedDictionariesNamesAndConnnectionStrings.ContainsKey(lc))
            {
                return true; // vacuous truth
            }

            foreach (Tuple<String, String> details in config.SavedDictionariesNamesAndConnnectionStrings[lc])
            {
                try
                {
                    ExternalDictionary dict = new ExternalDictionary(lc, details.Item1, details.Item2);
                    dict.Dispose();
                }
                catch (Exception e)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

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
        public static List<String> AvailableDictionaries( LanguageCode lc)
        {
            if (ConfigManager.SavedDictionariesNamesAndConnnectionStrings.ContainsKey(lc))
            {
                return ConfigManager.SavedDictionariesNamesAndConnnectionStrings[lc].Select(t => t.Item1).ToList();
            }
            else
            {
                return new List<String>();
            }
        }

        public static ExternalDictionary? GetDictionary(LanguageCode lc, String name)
        {
            if (!AvailableDictionaries(lc).Contains(name))
            {
                return null;
            }

            String connectionString = ConfigManager.SavedDictionariesNamesAndConnnectionStrings[lc].Where(t => t.Item1 == name).FirstOrDefault().Item2;

            return new ExternalDictionary(lc, name, connectionString);
        }

        public static void AddNewDictionaryFromCSV(LanguageCode lc, String name, String csvFileLocation)
        {
            if (AvailableDictionaries(lc).Contains(name))
            {
                throw new InvalidDataException("naming conflict identified, dictionary adding aborted");
            }

            String connectionString = ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(
                csvFileLocation, lc, name);

            UpdateConfigWithExistingDictionaryDatabase(lc, name, connectionString); 
        }

        public static void VerifyIntegrity()
        {
            if (!VerifyNoDuplicateDictionaries())
            {
                throw new Exception("Duplicate dictionaries found");
            }

            if (!VerifyExternalDictionariesExist())
            {
                throw new Exception("Expected dictionary not found");
            }
        }

        private static void UpdateConfigWithExistingDictionaryDatabase(LanguageCode lc, String name, String connectionString)
        {
            ExternalDictionary dict = new ExternalDictionary(lc, name, connectionString); // check that it exists
            dict.Dispose();

            if (ConfigManager.SavedDictionariesNamesAndConnnectionStrings.ContainsKey(lc))
            {
                if (ConfigManager.SavedDictionariesNamesAndConnnectionStrings[lc].Any(t => t.Item1 == name))
                {
                    throw new Exception("Already an existing dictionary with this name");
                }
            }

            ConfigManager.AddDictionaryDetails(lc, Tuple.Create(name, connectionString));
        }

        private static bool VerifyNoDuplicateDictionaries()
        {
            foreach (LanguageCode lc in ConfigManager.SavedDictionariesNamesAndConnnectionStrings.Keys)
            {
                List<String> names = AvailableDictionaries(lc);
                if (names.Distinct().Count() != names.Count())
                {
                    return false;
                }
            }
            return true;
        }

        private static bool VerifyExternalDictionariesExist()
        {
            // checks that all the dictionaries outlined in config do exist
            foreach (LanguageCode lc in ConfigManager.SavedDictionariesNamesAndConnnectionStrings.Keys)
            {
                if (!VerifyExternalDictionariesExist(lc))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool VerifyExternalDictionariesExist(LanguageCode lc)
        {
            // checks that all the dictionaries outlined in config do exist for a specific language

            if (!ConfigManager.SavedDictionariesNamesAndConnnectionStrings.ContainsKey(lc))
            {
                return true; // vacuous truth
            }

            foreach (Tuple<String, String> details in ConfigManager.SavedDictionariesNamesAndConnnectionStrings[lc])
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

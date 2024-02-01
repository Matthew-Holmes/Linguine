using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class ConfigManager
    {

        public static String FileStoreLocation
        {
            get => ConfigFileHandler.Copy.FileStoreLocation;
            set
            {
                Config tmp = ConfigFileHandler.Copy;
                tmp.FileStoreLocation = value;
                ConfigFileHandler.UpdateConfig(tmp);
            }
        }

        public static String DictionariesDirectory
        {
            get => ConfigFileHandler.Copy.DictionariesDirectory;
            set
            {
                Config tmp = ConfigFileHandler.Copy;
                tmp.DictionariesDirectory = value;
                ConfigFileHandler.UpdateConfig(tmp);
            }
        }

        public static LanguageCode NativeLanguage
        {
            get => ConfigFileHandler.Copy.NativeLanguage;
            set
            {
                Config tmp = ConfigFileHandler.Copy;
                tmp.NativeLanguage = value;
                ConfigFileHandler.UpdateConfig(tmp);
            }
        }

        public static LanguageCode TargetLanguage
        {
            get => ConfigFileHandler.Copy.TargetLanguage;
            set
            {
                Config tmp = ConfigFileHandler.Copy;
                tmp.TargetLanguage = value;
                ConfigFileHandler.UpdateConfig(tmp);
            }
        }

        public static Dictionary<LanguageCode, List<Tuple<String, String>>> SavedDictionariesNamesAndConnnectionStrings
        {
            get => ConfigFileHandler.Copy.SavedDictionariesNamesAndConnnectionStrings;
        }

        public static void AddDictionaryDetails(LanguageCode lc, Tuple<String,String> details)
        {
            Config tmp = ConfigFileHandler.Copy;
            if (tmp.SavedDictionariesNamesAndConnnectionStrings is null)
            {
                tmp.SavedDictionariesNamesAndConnnectionStrings = new Dictionary<LanguageCode, List<Tuple<string, string>>>();
            }
            if (tmp.SavedDictionariesNamesAndConnnectionStrings.ContainsKey(lc) is not true)
            {
                tmp.SavedDictionariesNamesAndConnnectionStrings[lc] = new List<Tuple<string, string>>();
            }

            tmp.SavedDictionariesNamesAndConnnectionStrings[lc].Add(details);

            ConfigFileHandler.UpdateConfig(tmp);
        }


        
    }
}
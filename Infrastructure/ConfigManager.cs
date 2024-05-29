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
        public static String OpenAI_APIKey
        {
            get => ConfigFileHandler.Copy.OpenAI_APIKeyLocation;
            set
            {
                Config tmp = ConfigFileHandler.Copy;
                tmp.OpenAI_APIKeyLocation = value;
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

        public static Dictionary<LanguageCode, String> ConnectionStrings
        {
            get => ConfigFileHandler.Copy.ConnectionStrings;
        }

        public static bool AddConnectionString(LanguageCode lc, String connectionString)
        {
            Config tmp = ConfigFileHandler.Copy;

            if (   tmp.ConnectionStrings.ContainsKey(lc) 
                && tmp.ConnectionStrings[lc] is not null 
                && tmp.ConnectionStrings[lc] != "")
            {
                return false;
            }

            tmp.ConnectionStrings[lc] = connectionString;

            ConfigFileHandler.UpdateConfig(tmp);

            return true;
        }
    }
}
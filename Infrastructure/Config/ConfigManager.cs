using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
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

        public static void SetLearnerLevel(LanguageCode lc, LearnerLevel ll)
        {
            Config tmp = ConfigFileHandler.Copy;
            tmp.LearnerLevels[lc] = ll;
            ConfigFileHandler.UpdateConfig(tmp);
        }

        public static LearnerLevel GetLearnerLevel(LanguageCode lc)
        {
            return ConfigFileHandler.Copy.LearnerLevels[lc];
        }


        public static String ConnectionString
        {
            get => ConnectionStrings[TargetLanguage];
        }
        
        public static String DatabaseDirectory
        {
            get => GetDatabaseSubdirectory(ConnectionString);
        }

        private static Dictionary<LanguageCode, String> ConnectionStrings
        {
            get => ConfigFileHandler.Copy.ConnectionStrings;
        }

        public static void ReplaceConnectionString(LanguageCode lc, String connectionString)
        {
            Config tmp = ConfigFileHandler.Copy;

            tmp.ConnectionStrings[lc] = connectionString;

            ConfigFileHandler.UpdateConfig(tmp);
        }

        public static string GetDatabaseSubdirectory(string connectionString)
        {
            try
            {
                DbConnectionStringBuilder builder = new DbConnectionStringBuilder();
                builder.ConnectionString = connectionString;

                if (builder.ContainsKey("data source"))
                {
                    string filePath = builder["data source"].ToString();
                    return Path.GetDirectoryName(filePath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }

            // Return null if database is not in a subdirectory or any error occurs
            return null;
        }
    }
}
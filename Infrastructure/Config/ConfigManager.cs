using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{

    public record API_Keys
    {
        public String? OpenAI_APIKey   { get; init; }
        public String? DeepSeek_APIKey { get; init; }
    }


    public static class ConfigManager
    {

        public static String API_Key_Locations
        {
            get
            {
                String openAI_keyLocation   = ConfigFileHandler.Copy.OpenAI_APIKeyLocation;
                String deepseek_keyLocation = ConfigFileHandler.Copy.DeepSeek_APIKeyLocation;

                return openAI_keyLocation + ", " + deepseek_keyLocation;
            }
        }


        public static API_Keys API_Keys
        {
            get
            {
                String openAI_keyLocation   = ConfigFileHandler.Copy.OpenAI_APIKeyLocation;
                String deepseek_keyLocation = ConfigFileHandler.Copy.DeepSeek_APIKeyLocation;

                String? openAI_key   = null;
                String? deepseek_key = null;

                if (File.Exists(openAI_keyLocation))
                {
                    openAI_key = File.ReadLines(openAI_keyLocation).FirstOrDefault();
                }

                if (File.Exists(deepseek_keyLocation))
                {
                    deepseek_key = File.ReadLines(deepseek_keyLocation).FirstOrDefault();
                }

                return new API_Keys
                {
                    OpenAI_APIKey   = openAI_key,
                    DeepSeek_APIKey = deepseek_key
                };

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

        public static LearnerLevel LearnerLevel
        {
            get => GetLearnerLevel(TargetLanguage);
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
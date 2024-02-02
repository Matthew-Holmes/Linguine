using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class ConfigFileHandler
    {
        public const String ConfigPath = "config.json";

        public delegate void ConfigUpdatedEventHandler();
        public static event ConfigUpdatedEventHandler ConfigUpdated;

        private static Config? _config = null;

        internal static Config Copy => _config?.Copy() ?? GenerateDefaultConfig();

        private static Config GenerateDefaultConfig()
        {
            Config config = new Config();

            config.FileStoreLocation = "Filestore/";
            config.DictionariesDirectory = "Dictionaries/";
            config.VariantsDirectory = "Variants/";

            config.OpenAI_APIKeyLocation = "APIKeys/OpenAI.txt";

            config.SavedDictionariesNamesAndConnnectionStrings = new Dictionary<LanguageCode, List<Tuple<string, string>>>();

            config.TargetLanguage = LanguageCode.eng;
            config.NativeLanguage = LanguageCode.eng;

            return config;
        }

        public static bool LoadConfig()
        {
            Config? loaded = LoadConfigFromFile(ConfigPath);

            if (loaded is not null)
            {
                _config = loaded;
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool SetCustomConfig(String path)
        {
            Config? loaded = LoadConfigFromFile(path);

            if (loaded is not null)
            {
                UpdateConfig(loaded); return true;
            }
            else
            {
                return false;
            }
        }

        public static void SetConfigToDefault()
        {
            UpdateConfig(GenerateDefaultConfig());
        }

        private static Config? LoadConfigFromFile(String path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }

            String json = File.ReadAllText(path);

            try
            {
                return JsonConvert.DeserializeObject<Config>(json);
            }
            catch
            {
                return null;
            }
        }

        private static readonly object _configLock = new object();
        internal static void UpdateConfig(Config newConfig)
        {
            if (_config?.Equals(newConfig) ?? false) { return; }

            string json = JsonConvert.SerializeObject(newConfig, Formatting.Indented);

            lock (_configLock)
            {
                File.WriteAllText(ConfigPath, json);
                _config = newConfig;
            }

            ConfigUpdated?.Invoke();
        }
    }
}

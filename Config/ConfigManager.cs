using System.Collections.Immutable;
using DataClasses;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Serilog;

namespace Config
{
    public record APIKeys(String? OpenAI_APIKey, String? DeepSeek_APIKey);

    public static class ConfigManager
    {
        private static readonly IConfigurationRoot _configuration;
        private static readonly String             _jsonConfigFile = "appsettings.json";
        private static readonly object             _lock           = new();
        private static          Config             _config;
        private static          APIKeys            _apiKeys;

        static ConfigManager()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(_jsonConfigFile, optional: false, reloadOnChange: true)
                .Build();

            _config = _configuration.Get<Config>() ?? new Config();

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/change-tokens?view=aspnetcore-9.0
            ChangeToken.OnChange(
                () => _configuration.GetReloadToken(),
                () => OnConfigChanged());

            LoadAPIKeys();
        }

        public static Config  Config  => _config;

        public static APIKeys APIKeys => _apiKeys;


        public static event Action ConfigChanged;


        private static void OnConfigChanged()
        {
            // get the _lock so we know that the file writing has finished
            // (if that was the source of the change)
            lock (_lock)
            {
                _config = _configuration.Get<Config>() ?? throw new Exception("couldn't parse new config");

                LoadAPIKeys();

                ConfigChanged?.Invoke();
            }
        }

        private static void LoadAPIKeys()
        {
            String? openAIKey   = ReadApiKeyFromFile(_config.APIKeys.OpenAI_APIKeyLocation);
            String? deepSeekKey = ReadApiKeyFromFile(_config.APIKeys.DeepSeek_APIKeyLocation);

            String googleKeyPath = _config.APIKeys.Google_ServiceAccountKeyLocation;

            if (!string.IsNullOrEmpty(googleKeyPath) && File.Exists(googleKeyPath))
            {
                Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", googleKeyPath);
            }
            else
            {
                Log.Warning("Warning: Google TTS API key file not found or not configured.");
            }

            _apiKeys = new APIKeys(openAIKey, deepSeekKey);
        }

        private static String? ReadApiKeyFromFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    Log.Warning("API key file not found: {FilePath}", filePath);
                    return null;
                }

                using (StreamReader reader = new(filePath))
                {
                    string? line = reader.ReadLine();
                    if (string.IsNullOrWhiteSpace(line))
                    {
                        Log.Warning("API key file {FilePath} is empty.", filePath);
                        return null;
                    }
                    return line;
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Couldn't read API key from {FilePath}", filePath);
                return null;
            }
        }

        public static void SaveConfig(Config config)
        {
            lock (_lock)
            {
                string json = System.Text.Json.JsonSerializer.Serialize(config, new System.Text.Json.JsonSerializerOptions
                {
                    WriteIndented = true
                });

                File.WriteAllText(_jsonConfigFile, json);

                Log.Information("config file updated");
            }
        }

        public static Config WithNewLearnerLevel(LearnerLevel learnerLevel)
        {
            Dictionary<LanguageCode, LearnerLevel > toAdd = _config.Languages.LearnerLevels.ToDictionary();
            toAdd[_config.Languages.TargetLanguage] = learnerLevel;

            IReadOnlyDictionary<LanguageCode, LearnerLevel> toAddImm = toAdd.ToImmutableDictionary();

            return _config with 
            { 
                Languages = _config.Languages with 
                { 
                    LearnerLevels = toAddImm
                } 
            };
        }
    }
}

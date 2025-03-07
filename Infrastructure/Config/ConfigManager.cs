using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Serilog;

namespace Infrastructure
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

        private static APIKeys LoadAPIKeys()
        {
            String? openAIKey   = ReadApiKeyFromFile(_config.APIKeys.OpenAI_APIKeyLocation);
            String? deepSeekKey = ReadApiKeyFromFile(_config.APIKeys.DeepSeek_APIKeyLocation);

            return new APIKeys(openAIKey, deepSeekKey);
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
    }
}

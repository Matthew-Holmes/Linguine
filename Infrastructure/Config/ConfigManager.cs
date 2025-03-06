using System;
using System.IO;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Serilog;

namespace Infrastructure
{
    public static class ConfigManager
    {
        private static readonly IConfigurationRoot _configuration;
        private static readonly String             _jsonConfigFile = "appsettings.json";
        private static readonly object             _lock           = new();

        static ConfigManager()
        {
            _configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile(_jsonConfigFile, optional: false, reloadOnChange: true)
                .Build();

            // https://learn.microsoft.com/en-us/aspnet/core/fundamentals/change-tokens?view=aspnetcore-9.0
            ChangeToken.OnChange(
                () => _configuration.GetReloadToken(),
                () => OnConfigChanged());
        }

        public static Config Config => _configuration.Get<Config>() ?? new Config();

        public static event Action ConfigChanged;

        private static void OnConfigChanged()
        {
            lock (_lock)
            {
                // get the _lock so we know that the file writing has finished
                // (if that was the source of the change)
                ConfigChanged?.Invoke();
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

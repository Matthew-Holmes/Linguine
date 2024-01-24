using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public static class ConfigManager
    {
        public const String ConfigPath = "config.json";

        public static Config? LoadConfig(String path = ConfigPath)
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

        public static bool UpdateConfig(Config newConfig)
        {
            try
            {
                string json = JsonConvert.SerializeObject(newConfig, Formatting.Indented);
                File.WriteAllText(ConfigPath, json);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public static Config GenerateDefaultConfig()
        {
            Config config = new Config();

            config.FileStoreLocation = "Filestore/";

            return config;
        }
    }
}
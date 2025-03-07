using Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;

namespace Tests_Infrastructure
{
    [TestClass]
    public class ConfigFileHandlerTests
    {
        public static bool ArePropertiesEqual<T>(T obj1, T obj2)
        {
            foreach (var property in typeof(T).GetProperties())
            {
                var value1 = property.GetValue(obj1);
                var value2 = property.GetValue(obj2);

                if (!Equals(value1, value2))
                    return false;
            }
            return true;
        }


        [TestInitialize]
        public void Initialize()
        {
            var currentDirectory = Directory.GetCurrentDirectory();

            // Check if the current directory contains "Tests_infrastructure" - so we know we're not touching actual config files!
            if (!currentDirectory.Contains("Tests_Infrastructure"))
            {
                throw new InvalidOperationException("Tests must be run in a directory containing 'Tests_Infrastructure'");
            }

            ConfigManager.SetConfigToDefault();

            if (File.Exists(ConfigManager.ConfigPath))
            {
                File.Delete(ConfigManager.ConfigPath);
            }
            if (File.Exists("customConfig.json"))
            {
                File.Delete("customConfig.json");
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            ConfigManager.SetConfigToDefault();

            if (File.Exists(ConfigManager.ConfigPath))
            {
                File.Delete(ConfigManager.ConfigPath);
            }
            if (File.Exists("customConfig.json"))
            {
                File.Delete("customConfig.json");
            }
        }

        [TestMethod]
        [ExpectedException(typeof(FileNotFoundException))]
        public void LoadConfig_WithNonExistingPath_ShouldThrowFileNotFoundException()
        {
            if (File.Exists(ConfigManager.ConfigPath))
            {
                File.Delete(ConfigManager.ConfigPath);
            }
            ConfigManager.LoadConfig();
        }

        [TestMethod]
        public void UpdateConfig_Filestore_Runs()
        {
            var newConfig = new Config();
            ConfigManager.UpdateConfig(newConfig);
        }

        [TestMethod]
        public void UpdateConfig_WithValidConfig_ShouldCreateFile()
        {
            var newConfig = new Config();
            ConfigManager.UpdateConfig(newConfig);

            Assert.IsTrue(File.Exists(ConfigManager.ConfigPath));
        }


        [TestMethod]
        public void LoadConfig_WithValidPath_ShouldReturnTrue()
        {
            var config = new Config();
            ConfigManager.UpdateConfig(config);

            bool loadedConfig = ConfigManager.LoadConfig();
            Assert.IsTrue(loadedConfig);
        }

        [TestMethod]
        public void LoadConfig_WithValidPath_ShouldLoadTheSame()
        {
            var config = new Config();
            ConfigManager.UpdateConfig(config);

            ConfigManager.LoadConfig();
            Assert.IsTrue(ConfigManager.Copy.Equals(config));
        }

        [TestMethod]
        public void LoadCustomConfig_WithValidPath_ShouldReturnTrue()
        {
            var config = new Config();
            ConfigManager.UpdateConfig(config);

            File.Copy(ConfigManager.ConfigPath, "customConfig.json");
            File.Delete(ConfigManager.ConfigPath);

            if (!File.Exists("customConfig.json"))
            {
                throw new Exception();
            }

            bool loadedConfig = ConfigManager.SetCustomConfig("customConfig.json");

            Assert.IsTrue(loadedConfig);
        }

        [TestMethod]
        public void LoadCustomConfig_WithValidPath_ShouldLoadTheSame()
        {
            var config = new Config();
            ConfigManager.UpdateConfig(config);

            File.Copy(ConfigManager.ConfigPath, "customConfig.json");
            File.Delete(ConfigManager.ConfigPath);

            ConfigManager.SetCustomConfig("customConfig.json");

            Assert.IsTrue(ConfigManager.Copy.Equals(config));
        }
    }


}

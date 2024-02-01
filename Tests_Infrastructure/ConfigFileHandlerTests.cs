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

            ConfigFileHandler.SetConfigToDefault();

            if (File.Exists(ConfigFileHandler.ConfigPath))
            {
                File.Delete(ConfigFileHandler.ConfigPath);
            }
            if (File.Exists("customConfig.json"))
            {
                File.Delete("customConfig.json");
            }
        }

        [TestCleanup]
        public void Cleanup()
        {
            ConfigFileHandler.SetConfigToDefault();

            if (File.Exists(ConfigFileHandler.ConfigPath))
            {
                File.Delete(ConfigFileHandler.ConfigPath);
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
            if (File.Exists(ConfigFileHandler.ConfigPath))
            {
                File.Delete(ConfigFileHandler.ConfigPath);
            }
            ConfigFileHandler.LoadConfig();
        }

        [TestMethod]
        public void UpdateConfig_Filestore_Runs()
        {
            var newConfig = new Config();
            ConfigFileHandler.UpdateConfig(newConfig);
        }

        [TestMethod]
        public void UpdateConfig_WithValidConfig_ShouldCreateFile()
        {
            var newConfig = new Config();
            ConfigFileHandler.UpdateConfig(newConfig);

            Assert.IsTrue(File.Exists(ConfigFileHandler.ConfigPath));
        }


        [TestMethod]
        public void LoadConfig_WithValidPath_ShouldReturnTrue()
        {
            var config = new Config { FileStoreLocation = "TestLocation/" };
            ConfigFileHandler.UpdateConfig(config);

            bool loadedConfig = ConfigFileHandler.LoadConfig();
            Assert.IsTrue(loadedConfig);
        }

        [TestMethod]
        public void LoadConfig_WithValidPath_ShouldLoadTheSame()
        {
            var config = new Config { FileStoreLocation = "TestLocation/" };
            ConfigFileHandler.UpdateConfig(config);

            ConfigFileHandler.LoadConfig();
            Assert.IsTrue(ConfigFileHandler.Copy.Equals(config));
        }

        [TestMethod]
        public void LoadCustomConfig_WithValidPath_ShouldReturnTrue()
        {
            var config = new Config { FileStoreLocation = "TestLocation/" };
            ConfigFileHandler.UpdateConfig(config);

            File.Copy(ConfigFileHandler.ConfigPath, "customConfig.json");
            File.Delete(ConfigFileHandler.ConfigPath);

            if (!File.Exists("customConfig.json"))
            {
                throw new Exception();
            }

            bool loadedConfig = ConfigFileHandler.SetCustomConfig("customConfig.json");

            Assert.IsTrue(loadedConfig);
        }

        [TestMethod]
        public void LoadCustomConfig_WithValidPath_ShouldLoadTheSame()
        {
            var config = new Config { FileStoreLocation = "TestLocation/" };
            ConfigFileHandler.UpdateConfig(config);

            File.Copy(ConfigFileHandler.ConfigPath, "customConfig.json");
            File.Delete(ConfigFileHandler.ConfigPath);

            ConfigFileHandler.SetCustomConfig("customConfig.json");

            Assert.IsTrue(ConfigFileHandler.Copy.Equals(config));
        }
    }


}

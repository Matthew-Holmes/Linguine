using Microsoft.VisualStudio.TestTools.UnitTesting;
using Infrastructure;

namespace Tests_Infrastructure
{
    [TestClass]
    public class ConfigManagerTests
    {
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
        }

        [TestCleanup]
        public void Cleanup()
        {
            ConfigFileHandler.SetConfigToDefault();

            if (File.Exists(ConfigFileHandler.ConfigPath))
            {
                File.Delete(ConfigFileHandler.ConfigPath);
            }
        }


        [TestMethod]
        public void TestOpenAIAPIKeyGetSet()
        {
            string testValue = "keys/somethingelse.txt";
            ConfigManager.OpenAI_APIKey = testValue;
            Assert.AreEqual(testValue, ConfigManager.OpenAI_APIKey);
        }

        [TestMethod]
        public void TestNativeLanguageGetSet()
        {
            LanguageCode testValue = LanguageCode.eng;
            ConfigManager.NativeLanguage = testValue;
            Assert.AreEqual(testValue, ConfigManager.NativeLanguage);
        }

        [TestMethod]
        public void TestTargetLanguageGetSet()
        {
            LanguageCode testValue = LanguageCode.fra;
            ConfigManager.TargetLanguage = testValue;
            Assert.AreEqual(testValue, ConfigManager.TargetLanguage);
        }

        [TestMethod]
        public void TestAddConnectionStringReturnsTrueIfValid()
        {
            LanguageCode lc = LanguageCode.zho;

            bool ret = ConfigManager.AddConnectionString(lc, "conn string");

            Assert.IsTrue(ret);
        }

        [TestMethod]
        public void TestAddConnectionStringReturnsFalseIfInvalid()
        {
            LanguageCode lc = LanguageCode.zho;

            bool ret = ConfigManager.AddConnectionString(lc, "conn string");
            bool ret2 = ConfigManager.AddConnectionString(lc, "new conn string");

            Assert.IsTrue(ret);
            Assert.IsFalse(ret2);
        }


        [TestMethod]
        public void TestAddConnectionStringReturnsTrueIfOverridingEmpty()
        {
            LanguageCode lc = LanguageCode.zho;

            bool ret = ConfigManager.AddConnectionString(lc, "");
            bool ret2 = ConfigManager.AddConnectionString(lc, "new conn string");

            Assert.IsTrue(ret2);
        }

        [TestMethod]
        public void TestAddConnectionString()
        {
            LanguageCode lc = LanguageCode.zho;

            bool ret = ConfigManager.AddConnectionString(lc, "conn string");
            
            Assert.IsTrue(ConfigManager.ConnectionStrings.ContainsKey(lc));
            Assert.IsTrue(ConfigManager.ConnectionStrings[lc] == "conn string");
        }

    }
}


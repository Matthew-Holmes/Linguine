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
        public void TestFileStoreLocationGetSet()
        {
            string testValue = "test/path";
            ConfigManager.FileStoreLocation = testValue;
            Assert.AreEqual(testValue, ConfigManager.FileStoreLocation);
        }

        [TestMethod]
        public void TestDictionariesDirectoryGetSet()
        {
            string testValue = "test/dictionaries";
            ConfigManager.DictionariesDirectory = testValue;
            Assert.AreEqual(testValue, ConfigManager.DictionariesDirectory);
        }

        [TestMethod]
        public void TestVariantsDirectoryGetSet()
        {
            string testValue = "test/variants";
            ConfigManager.VariantsDirectory = testValue;
            Assert.AreEqual(testValue, ConfigManager.VariantsDirectory);
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
        public void TestAddDictionaryDetails()
        {
            var details = new Tuple<string, string>("name", "connectionString");
            LanguageCode lc = LanguageCode.zho;

            ConfigManager.AddDictionaryDetails(lc, details);

            Assert.IsTrue(ConfigManager.SavedDictionariesNamesAndConnnectionStrings.ContainsKey(lc));
            Assert.IsTrue(ConfigManager.SavedDictionariesNamesAndConnnectionStrings[lc].Contains(details));
        }

        [TestMethod]
        public void TestAddMultipleDictionaryDetails()
        {
            var details1 = new Tuple<string, string>("name1", "connectionString1");
            var details2 = new Tuple<string, string>("name2", "connectionString2");
            var details3 = new Tuple<string, string>("name3", "connectionString3");
            LanguageCode lc = LanguageCode.zho;

            ConfigManager.AddDictionaryDetails(lc, details1);
            ConfigManager.AddDictionaryDetails(lc, details2);
            ConfigManager.AddDictionaryDetails(lc, details3);

            Assert.IsTrue(ConfigManager.SavedDictionariesNamesAndConnnectionStrings.ContainsKey(lc));
            Assert.AreEqual(3, ConfigManager.SavedDictionariesNamesAndConnnectionStrings[lc].Count);
            Assert.IsTrue(ConfigManager.SavedDictionariesNamesAndConnnectionStrings[lc].Contains(details1));
            Assert.IsTrue(ConfigManager.SavedDictionariesNamesAndConnnectionStrings[lc].Contains(details2));
            Assert.IsTrue(ConfigManager.SavedDictionariesNamesAndConnnectionStrings[lc].Contains(details3));
        }

        [TestMethod]
        public void TestAddVariantsDetails()
        {
            var details = new Tuple<string, string>("name", "connectionString");
            LanguageCode lc = LanguageCode.zho;

            ConfigManager.AddVariantsDetails(lc, details);

            Assert.IsTrue(ConfigManager.SavedVariantsNamesAndConnnectionStrings.ContainsKey(lc));
            Assert.IsTrue(ConfigManager.SavedVariantsNamesAndConnnectionStrings[lc].Contains(details));
        }

        [TestMethod]
        public void TestAddMultipleVariantsDetails()
        {
            var details1 = new Tuple<string, string>("name1", "connectionString1");
            var details2 = new Tuple<string, string>("name2", "connectionString2");
            var details3 = new Tuple<string, string>("name3", "connectionString3");
            LanguageCode lc = LanguageCode.zho;

            ConfigManager.AddVariantsDetails(lc, details1);
            ConfigManager.AddVariantsDetails(lc, details2);
            ConfigManager.AddVariantsDetails(lc, details3);

            Assert.IsTrue(ConfigManager.SavedVariantsNamesAndConnnectionStrings.ContainsKey(lc));
            Assert.AreEqual(3, ConfigManager.SavedVariantsNamesAndConnnectionStrings[lc].Count);
            Assert.IsTrue(ConfigManager.SavedVariantsNamesAndConnnectionStrings[lc].Contains(details1));
            Assert.IsTrue(ConfigManager.SavedVariantsNamesAndConnnectionStrings[lc].Contains(details2));
            Assert.IsTrue(ConfigManager.SavedVariantsNamesAndConnnectionStrings[lc].Contains(details3));
        }
    }
}


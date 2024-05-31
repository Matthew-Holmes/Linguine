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
        public void TestReplaceConnectionStringReturnsTrueIfOverridingEmpty()
        {
            LanguageCode lc = LanguageCode.zho;

            ConfigManager.ReplaceConnectionString(lc, "yi xin db");
            ConfigManager.TargetLanguage = LanguageCode.zho;

            Assert.AreEqual(ConfigManager.ConnectionString, "yi xin db");
        }

        [TestMethod]
        public void GetDatabaseSubdirectory_ValidConnectionString_ReturnsDirectory()
        {
            // Arrange
            string connectionString = "data source=C:\\MyDatabase\\mydatabase.db";

            // Act
            string result = ConfigManager.GetDatabaseSubdirectory(connectionString);

            // Assert
            Assert.AreEqual("C:\\MyDatabase", result);
        }

        [TestMethod]
        public void GetDatabaseSubdirectory_InvalidConnectionString_ReturnsNull()
        {
            // Arrange
            string connectionString = "invalid connection string";

            // Act
            string result = ConfigManager.GetDatabaseSubdirectory(connectionString);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetDatabaseSubdirectory_ConnectionStringWithoutDataSourceKey_ReturnsNull()
        {
            // Arrange
            string connectionString = "some other key=value";

            // Act
            string result = ConfigManager.GetDatabaseSubdirectory(connectionString);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetDatabaseSubdirectory_NullConnectionString_ReturnsNull()
        {
            // Arrange
            string connectionString = null;

            // Act
            string result = ConfigManager.GetDatabaseSubdirectory(connectionString);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetDatabaseSubdirectory_EmptyConnectionString_ReturnsNull()
        {
            // Arrange
            string connectionString = "";

            // Act
            string result = ConfigManager.GetDatabaseSubdirectory(connectionString);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetDatabaseSubdirectory_ValidRelativeFilePath_ReturnsDirectory()
        {
            // Arrange
            string relativePath = "MyDatabase\\mydatabase.db";
            string currentDirectory = Directory.GetCurrentDirectory();
            string connectionString = $"data source={relativePath}";

            // Act
            string result =ConfigManager.GetDatabaseSubdirectory(connectionString);

            // Assert
            Assert.AreEqual("MyDatabase", result);
        }
    }
}


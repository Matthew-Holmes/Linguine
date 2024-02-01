using Infrastructure;
using LearningStore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LearningStoreTests
{
    [TestClass]
    public class ExternalDictionaryManagerTests
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            InitializeDummyDatabases();
            testStartState = ConfigFileHandler.Copy;
        }

        private static Config testStartState;

        private static string CreateMockCSVFile()
        {
            string filePath = Path.Combine("dummyRawData.csv");
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("ID,Word,Definition"); // CSV headers
            csvContent.AppendLine("0,TestWord0,TestDefinition0");
            csvContent.AppendLine("1,TestWord1,TestDefinition1");
            csvContent.AppendLine("2,TestWord2,TestDefinition2");

            File.WriteAllText(filePath, csvContent.ToString(), Encoding.Unicode);

            return filePath;
        }


        private static void InitializeDummyDatabases()
        {
            String dummyData = CreateMockCSVFile();

            ConfigFileHandler.SetConfigToDefault();
            ConfigManager.FileStoreLocation = "DummyFileStore";
            ConfigManager.DictionariesDirectory = "DummyDictionaries";

            String eng1ConnectionString = ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(dummyData, LanguageCode.eng, "EnglishDict1");
            String eng2ConnectionString = ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(dummyData, LanguageCode.eng, "EnglishDict2");
            String zhoConnectionString = ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(dummyData, LanguageCode.zho, "ChineseDict");

            ConfigManager.AddDictionaryDetails(LanguageCode.eng, Tuple.Create("English1", eng1ConnectionString));
            ConfigManager.AddDictionaryDetails(LanguageCode.eng, Tuple.Create("English2", eng2ConnectionString));
            ConfigManager.AddDictionaryDetails(LanguageCode.zho, Tuple.Create("Chinese", zhoConnectionString));
        }


        [TestMethod]
        public void AvailableDictionaries_ReturnsCorrectDictionaryNames()
        {
            var expected = new List<string> { "English1", "English2" };
            var result = ExternalDictionaryManager.AvailableDictionaries(LanguageCode.eng);
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AvailableDictionaries_ReturnsCorrectDictionaryNamesWhenNone()
        {
            var expected = new List<string>();
            var result = ExternalDictionaryManager.AvailableDictionaries(LanguageCode.fra);
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetDictionary_ReturnsDictionaryIfExists()
        {
            string expectedName = "English1";
            LanguageCode lc = LanguageCode.eng;
            ExternalDictionary result = ExternalDictionaryManager.GetDictionary(lc, expectedName);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedName, result.Name);
        }

        [TestMethod]
        public void GetDictionary_ReturnsNullIfDictionaryDoesNotExist()
        {
            string name = "NonExistingDict";
            LanguageCode lc = LanguageCode.eng;

            ExternalDictionary result = ExternalDictionaryManager.GetDictionary(lc, name);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetDictionary_ReturnsNullIfDictionaryReallyDoesNotExist()
        {
            string name = "NonExistingDict";
            LanguageCode lc = LanguageCode.fra; // we don't even have any of these

            ExternalDictionary result = ExternalDictionaryManager.GetDictionary(lc, name);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void AddNewDictionaryFromCSV_AddsDictionarySuccessfully()
        {
            string name = "NewDict";
            LanguageCode lc = LanguageCode.zho;
            string csvFileLocation = CreateMockCSVFile();

            ExternalDictionaryManager.AddNewDictionaryFromCSV(lc, name, csvFileLocation);

            var dictionaries = ExternalDictionaryManager.AvailableDictionaries(lc);
            Assert.IsTrue(dictionaries.Contains(name));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void AddNewDictionaryFromCSV_ThrowsExceptionForDuplicateName()
        {
            string name = "English1"; // Existing name
            LanguageCode lc = LanguageCode.eng;
            string csvFileLocation = CreateMockCSVFile();

            ExternalDictionaryManager.AddNewDictionaryFromCSV(lc, name, csvFileLocation);
        }


        [TestMethod]
        [ExpectedException(typeof(Exception), "Duplicate dictionaries found")]
        public void VerifyIntegrity_ThrowsExceptionForDuplicateDictionaries()
        {
            // Add a duplicate dictionary to a tmp for testing
            ConfigManager.AddDictionaryDetails(LanguageCode.eng, Tuple.Create("English1", "CanBeDifferentConnectionString"));

            ExternalDictionaryManager.VerifyIntegrity();

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Expected dictionary not found")]
        public void VerifyIntegrityWith_ThrowsExceptionForNonexistantDictionary()
        {
            ConfigManager.AddDictionaryDetails(LanguageCode.eng, Tuple.Create("Nonexistant", "doesntmatter"));
            ExternalDictionaryManager.VerifyIntegrity();

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        public void VerifyIntegrityWith_PassesForValidConfig()
        {
            ExternalDictionaryManager.VerifyIntegrity();
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (!ConfigFileHandler.Copy.Equals(testStartState))
            {
                InitializeDummyDatabases();
            }
        }

        [ClassCleanup]
        public static void ClassCleanUp()
        {
            try
            {
                // Delete the dummyRawData.csv file
                string csvFilePath = Path.Combine("dummyRawData.csv");
                if (File.Exists(csvFilePath))
                {
                    File.Delete(csvFilePath);
                }

                string directoryPath = ConfigManager.FileStoreLocation;
                if (Directory.Exists(directoryPath))
                {
                    // Delete all files and subdirectories in the directory
                    Directory.Delete(directoryPath, true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during cleanup: " + ex.Message);
                // You might want to log this exception or handle it as needed
            }
        }

    }
}

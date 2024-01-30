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
        private static Config Config { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
           Config = new Config
            {
                FileStoreLocation = "DummyFileStore",
                DictionariesDirectory = "DummyDictionaries",
                SavedDictionariesNamesAndConnnectionStrings = new Dictionary<LanguageCode, List<Tuple<string, string>>>()
            };

            Config.SavedDictionariesNamesAndConnnectionStrings[LanguageCode.eng] = new List<Tuple<string, string>>();
            Config.SavedDictionariesNamesAndConnnectionStrings[LanguageCode.zho] = new List<Tuple<string, string>>();


            String dummyData = CreateMockCSVFile();
            InitializeDummyDatabases(dummyData);
        }

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

        private static void InitializeDummyDatabases(String dummyData)
        {
            String eng1ConnectionString = ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(dummyData, Config, LanguageCode.eng, "EnglishDict1");
            String eng2ConnectionString = ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(dummyData, Config, LanguageCode.eng, "EnglishDict2");
            String zhoConnectionString = ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(dummyData, Config, LanguageCode.zho, "ChineseDict");

            Config.SavedDictionariesNamesAndConnnectionStrings[LanguageCode.eng].Add(Tuple.Create("English1", eng1ConnectionString));
            Config.SavedDictionariesNamesAndConnnectionStrings[LanguageCode.eng].Add(Tuple.Create("English2", eng2ConnectionString));
            Config.SavedDictionariesNamesAndConnnectionStrings[LanguageCode.zho].Add(Tuple.Create("Chinese", zhoConnectionString));
        }


        [TestMethod]
        public void AvailableDictionaries_ReturnsCorrectDictionaryNames()
        {
            var expected = new List<string> { "English1", "English2" };
            var result = ExternalDictionaryManager.AvailableDictionaries(Config, LanguageCode.eng);
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AvailableDictionaries_ReturnsCorrectDictionaryNamesWhenNone()
        {
            var expected = new List<string>();
            var result = ExternalDictionaryManager.AvailableDictionaries(Config, LanguageCode.fra);
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetDictionary_ReturnsDictionaryIfExists()
        {
            string expectedName = "English1";
            LanguageCode lc = LanguageCode.eng;
            ExternalDictionary result = ExternalDictionaryManager.GetDictionary(Config, lc, expectedName);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedName, result.Name);
        }

        [TestMethod]
        public void GetDictionary_ReturnsNullIfDictionaryDoesNotExist()
        {
            string name = "NonExistingDict";
            LanguageCode lc = LanguageCode.eng;

            ExternalDictionary result = ExternalDictionaryManager.GetDictionary(Config, lc, name);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetDictionary_ReturnsNullIfDictionaryReallyDoesNotExist()
        {
            string name = "NonExistingDict";
            LanguageCode lc = LanguageCode.fra; // we don't even have any of these

            ExternalDictionary result = ExternalDictionaryManager.GetDictionary(Config, lc, name);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void AddNewDictionaryFromCSV_AddsDictionarySuccessfully()
        {
            string name = "NewDict";
            LanguageCode lc = LanguageCode.zho;
            string csvFileLocation = CreateMockCSVFile();

            ExternalDictionaryManager.AddNewDictionaryFromCSV(Config, lc, name, csvFileLocation);

            var dictionaries = ExternalDictionaryManager.AvailableDictionaries(Config, lc);
            Assert.IsTrue(dictionaries.Contains(name));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void AddNewDictionaryFromCSV_ThrowsExceptionForDuplicateName()
        {
            string name = "English1"; // Existing name
            LanguageCode lc = LanguageCode.eng;
            string csvFileLocation = CreateMockCSVFile();

            ExternalDictionaryManager.AddNewDictionaryFromCSV(Config, lc, name, csvFileLocation);
        }


        [TestMethod]
        [ExpectedException(typeof(Exception), "Duplicate dictionaries found")]
        public void VerifyIntegrityWith_ThrowsExceptionForDuplicateDictionaries()
        {
            // Add a duplicate dictionary to a tmp for testing
            Config tmp = Config.Copy();
            tmp.SavedDictionariesNamesAndConnnectionStrings[LanguageCode.eng].Add(Tuple.Create("English1", "CanBeDifferentConnectionString"));

            ExternalDictionaryManager.VerifyIntegrityWith(tmp);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Expected dictionary not found")]
        public void VerifyIntegrityWith_ThrowsExceptionForNonexistantDictionary()
        {
            // Add a duplicate dictionary to a tmp for testing
            Config tmp = Config.Copy();
            tmp.SavedDictionariesNamesAndConnnectionStrings[LanguageCode.eng].Add(Tuple.Create("Nonexistant", "CanBeDifferentConnectionString"));

            ExternalDictionaryManager.VerifyIntegrityWith(tmp);

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        public void VerifyIntegrityWith_PassesForValidConfig()
        {
            ExternalDictionaryManager.VerifyIntegrityWith(Config);
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

                string directoryPath = Config.FileStoreLocation;
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

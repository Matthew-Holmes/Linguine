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
    public class VariantsManagerTests
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
            string filePath = Path.Combine("dummyVariantsData.csv");
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("Variant,Root"); // CSV headers
            csvContent.AppendLine("variant1,root1");
            csvContent.AppendLine("variant2,root1");
            csvContent.AppendLine("variant3,root2");

            File.WriteAllText(filePath, csvContent.ToString(), Encoding.Unicode);

            return filePath;
        }

        private static void InitializeDummyDatabases()
        {
            String dummyData = CreateMockCSVFile();

            ConfigFileHandler.SetConfigToDefault();
            ConfigManager.FileStoreLocation = "DummyFileStore";
            ConfigManager.VariantsDirectory = "DummyVariants";

            String eng1ConnectionString = VariantsCSVParser.ParseVariantsFromCSVToSQLiteAndSave(dummyData, LanguageCode.eng, "EnglishVariants1");
            String eng2ConnectionString = VariantsCSVParser.ParseVariantsFromCSVToSQLiteAndSave(dummyData, LanguageCode.eng, "EnglishVariants2");
            String zhoConnectionString = VariantsCSVParser.ParseVariantsFromCSVToSQLiteAndSave(dummyData, LanguageCode.zho, "ChineseVariants");

            ConfigManager.AddVariantsDetails(LanguageCode.eng, Tuple.Create("English1", eng1ConnectionString));
            ConfigManager.AddVariantsDetails(LanguageCode.eng, Tuple.Create("English2", eng2ConnectionString));
            ConfigManager.AddVariantsDetails(LanguageCode.zho, Tuple.Create("Chinese", zhoConnectionString));
        }

        // ... (Continuation of VariantsManagerTests class)

        [TestMethod]
        public void AvailableVariants_ReturnsCorrectVariantNames()
        {
            var expected = new List<string> { "English1", "English2" };
            var result = VariantsManager.AvailableVariantsSources(LanguageCode.eng);
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void AvailableVariants_ReturnsCorrectVariantNamesWhenNone()
        {
            var expected = new List<string>();
            var result = VariantsManager.AvailableVariantsSources(LanguageCode.fra);
            CollectionAssert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetVariants_ReturnsVariantsIfExists()
        {
            string expectedName = "English1";
            LanguageCode lc = LanguageCode.eng;
            Variants result = VariantsManager.GetVariantsSource(lc, expectedName);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedName, result.Name);
        }

        [TestMethod]
        public void GetVariants_ReturnsNullIfVariantsDoesNotExist()
        {
            string name = "NonExistingVariants";
            LanguageCode lc = LanguageCode.eng;

            Variants result = VariantsManager.GetVariantsSource(lc, name);
            Assert.IsNull(result);
        }

        [TestMethod]
        public void AddNewVariantsFromCSV_AddsVariantsSuccessfully()
        {
            string name = "NewVariants";
            LanguageCode lc = LanguageCode.zho;
            string csvFileLocation = CreateMockCSVFile();

            VariantsManager.AddNewVariantsSourceFromCSV(lc, name, csvFileLocation);

            var variants = VariantsManager.AvailableVariantsSources(lc);
            Assert.IsTrue(variants.Contains(name));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void AddNewVariantsFromCSV_ThrowsExceptionForDuplicateName()
        {
            string name = "English1"; // Existing name
            LanguageCode lc = LanguageCode.eng;
            string csvFileLocation = CreateMockCSVFile();

            VariantsManager.AddNewVariantsSourceFromCSV(lc, name, csvFileLocation);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Duplicate variants found")]
        public void VerifyIntegrity_ThrowsExceptionForDuplicateVariants()
        {
            // Add a duplicate variants to a tmp for testing
            ConfigManager.AddVariantsDetails(LanguageCode.eng, Tuple.Create("English1", "CanBeDifferentConnectionString"));

            VariantsManager.VerifyIntegrity();

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Expected variant sources not found")]
        public void VerifyIntegrityWith_ThrowsExceptionForNonexistantVariants()
        {
            ConfigManager.AddVariantsDetails(LanguageCode.eng, Tuple.Create("Nonexistant", "doesntmatter"));
            VariantsManager.VerifyIntegrity();

            // Assert is handled by ExpectedException
        }

        [TestMethod]
        public void VerifyIntegrityWith_PassesForValidConfig()
        {
            VariantsManager.VerifyIntegrity();
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

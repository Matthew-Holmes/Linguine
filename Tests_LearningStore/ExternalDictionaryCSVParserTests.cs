using Infrastructure;
using LearningStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace LearningStoreTests
{
    [TestClass]
    public class ExternalDictionaryCSVParserTests
    {
        private string _csvFilePath;
        private LanguageCode _testLanguageCode;
        private string _testDictionaryName;
        private string _databaseFilePath;

        [TestInitialize]
        public void SetUp()
        {
            _testLanguageCode = LanguageCode.eng; 
            _testDictionaryName = "TestDictionary";
            _databaseFilePath = "testDatabase.db";

            // Create a mock CSV file with test data
            _csvFilePath = CreateMockCSVFile();
        }

        private string CreateMockCSVFile()
        {
            string filePath = Path.Combine("testRawData.csv");
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("ID,Word,Definition"); // CSV headers
            csvContent.AppendLine("0,TestWord0,TestDefinition0");
            csvContent.AppendLine("1,TestWord1,TestDefinition1");
            csvContent.AppendLine("2,TestWord2,TestDefinition2");

            File.WriteAllText(filePath, csvContent.ToString(), Encoding.Unicode);

            return filePath;
        }


        [TestMethod]
        public void ParseNewDictionaryFromCSV_ValidCSV_Runs()
        {
            ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(_csvFilePath, _testLanguageCode, _testDictionaryName);
        }

        [TestMethod]
        public void ParseNewDictionaryFromCSV_ValidCSV_CreatesDatabase()
        {
            String expectedConnectionString = ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(
                _csvFilePath, _testLanguageCode, _testDictionaryName);

            using (ExternalDictionaryContext context = new ExternalDictionaryContext(expectedConnectionString))
            {
                var dictionaryEntry0 = context.DictionaryDefinitions.FirstOrDefault(dd => dd.Word == "TestWord0");
                Assert.IsNotNull(dictionaryEntry0);
                Assert.AreEqual(dictionaryEntry0.Definition, "TestDefinition0");

                var dictionaryEntry1 = context.DictionaryDefinitions.FirstOrDefault(dd => dd.Word == "TestWord1");
                Assert.IsNotNull(dictionaryEntry1);
                Assert.AreEqual(dictionaryEntry1.Definition, "TestDefinition1");

                var dictionaryEntry2 = context.DictionaryDefinitions.FirstOrDefault(dd => dd.Word == "TestWord2");
                Assert.IsNotNull(dictionaryEntry2);
                Assert.AreEqual(dictionaryEntry2.Definition, "TestDefinition2");
            }
            // Additional assertions to verify database content can be added here
        }

        [TestMethod]
        [ExpectedException(typeof(DataException))]
        public void ParseNewDictionaryFromCSV_EmptyCSV_ThrowsException()
        {
            // Create an empty CSV file
            string emptyCsvFilePath = CreateEmptyCSVFile();

            ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(emptyCsvFilePath,  _testLanguageCode, _testDictionaryName);
        }

        [TestMethod]
        [ExpectedException(typeof(DataException))]
        public void ParseNewDictionaryFromCSV_CompletelyEmptyCSV_ThrowsException()
        {
            // Create an empty CSV file
            string emptyCsvFilePath = CreateCompletelyEmptyCSVFile();

            ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(emptyCsvFilePath,  _testLanguageCode, _testDictionaryName);
        }

        private string CreateCompletelyEmptyCSVFile()
        {
            string filePath = Path.Combine("completelyEmpty.csv");
            File.WriteAllText(filePath, "", Encoding.Unicode);

            return filePath;
        }

        private string CreateEmptyCSVFile()
        {
            string filePath = Path.Combine("empty.csv");
            File.WriteAllText(filePath, "ID,Word,Definition", Encoding.Unicode);

            return filePath;
        }

        [TestCleanup]
        public void CleanUp()
        {
            // Delete the temporary CSV file
            if (File.Exists(_csvFilePath))
            {
                File.Delete(_csvFilePath);
            }

            String connectionString = $"Data Source={_databaseFilePath};";

            // Create the database and schema
            ExternalDictionaryContext context = new ExternalDictionaryContext(connectionString);
            context.Database.EnsureDeleted();

            if (File.Exists(_databaseFilePath))
            {
                throw new Exception();
            }
        }
    }
}

using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace Tests_Infrastructure
{
    [TestClass]
    public class ExternalDictionaryCSVParserTests
    {
        private const string ConnectionString = $"Data Source=tmp.db;";
        private LinguineDataHandler _db;

        private string _csvFilePath;
        private string _testDictionaryName;

        private void SeedData()
        {
        _db = new LinguineDataHandler(ConnectionString);
        _db.Database.EnsureCreated();
        }

        [TestInitialize]
        public void SetUp()
        {
            using (var _db = new LinguineDataHandler(ConnectionString))
            {
                _db.Database.EnsureDeleted(); // use this way as File method doesn't work
            }

            if (File.Exists("tmp.db"))
            {
                throw new Exception();
            }

            SeedData();

            _testDictionaryName = "TestDictionary";

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
            ExternalDictionary dictionary = new ExternalDictionary(_testDictionaryName, _db);

            ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(dictionary, _csvFilePath, _testDictionaryName);
        }

        [TestMethod]
        public void ParseNewDictionaryFromCSV_ValidCSV_AddsDefinitions()
        {
            ExternalDictionary dictionary = new ExternalDictionary(_testDictionaryName, _db);

            ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(dictionary, _csvFilePath, _testDictionaryName);

            Assert.IsTrue(dictionary.Contains("TestWord0"));
            var dictionaryEntry0 = dictionary.TryGetDefinition("TestWord0").First();
            Assert.IsNotNull(dictionaryEntry0);
            Assert.AreEqual(dictionaryEntry0.Definition, "TestDefinition0");

            Assert.IsTrue(dictionary.Contains("TestWord1"));
            var dictionaryEntry1 = dictionary.TryGetDefinition("TestWord1").First();
            Assert.IsNotNull(dictionaryEntry1);
            Assert.AreEqual(dictionaryEntry1.Definition, "TestDefinition1");

            Assert.IsTrue(dictionary.Contains("TestWord2"));
            var dictionaryEntry2 = dictionary.TryGetDefinition("TestWord2").First();
            Assert.IsNotNull(dictionaryEntry2);
            Assert.AreEqual(dictionaryEntry2.Definition, "TestDefinition2");
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void ParseNewDictionaryFromCSV_NameMismatch_ThrowsException()
        {
            // Create an empty CSV file
            string emptyCsvFilePath = CreateEmptyCSVFile();

            ExternalDictionary target = new ExternalDictionary("newDict", _db);

            ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(target, emptyCsvFilePath, "anotherDict");
        }

        [TestMethod]
        [ExpectedException(typeof(DataException))]
        public void ParseNewDictionaryFromCSV_EmptyCSV_ThrowsException()
        {
            // Create an empty CSV file
            string emptyCsvFilePath = CreateEmptyCSVFile();

            ExternalDictionary target = new ExternalDictionary("newDict", _db);

            ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(target, emptyCsvFilePath, "newDict");
        }

        [TestMethod]
        [ExpectedException(typeof(DataException))]
        public void ParseNewDictionaryFromCSV_CompletelyEmptyCSV_ThrowsException()
        {
            // Create an empty CSV file
            string emptyCsvFilePath = CreateCompletelyEmptyCSVFile();

            ExternalDictionary dictionary = new ExternalDictionary(_testDictionaryName, _db);

            ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(dictionary, emptyCsvFilePath, _testDictionaryName);
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

            using (var _db = new LinguineDataHandler(ConnectionString))
            {
                _db.Database.EnsureDeleted(); // use this way as File method doesn't work
            }

            if (File.Exists("tmp.db"))
            {
                throw new Exception();
            }
        }
    }
}

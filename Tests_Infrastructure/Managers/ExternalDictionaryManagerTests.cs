﻿using Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tests_Infrastructure
{
    [TestClass]
    public class ExternalDictionaryManagerTests
    {
        private const string ConnectionString = $"Data Source=tmp.db;";
        private LinguineDbContextFactory _dbf;

        private string dummyDataFile;

        [ClassInitialize]
        public static void ClassInitialize(TestContext tc)
        {
            CreateMockCSVFile();
        }

        [TestInitialize]
        public void TestInitialize()
        {
            using (var _db = new LinguineDbContext(ConnectionString))
            {
                _db.Database.EnsureDeleted(); // use this way as File method doesn't work
            }


            if (File.Exists("tmp.db"))
            {
                throw new Exception();
            }

            _dbf = new LinguineDbContextFactory(ConnectionString);

            var context = _dbf.CreateDbContext();
            context.Database.EnsureCreated();

            dummyDataFile = Path.Combine("dummyRawData.csv");

            InitializeDummyDictionaries();

            context.Dispose();
            
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


        private void InitializeDummyDictionaries()
        {

            ExternalDictionary target1 = new ExternalDictionary("English1", _dbf);
            ExternalDictionary target2 = new ExternalDictionary("English2", _dbf);

            ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(target1, dummyDataFile, "English1");
            ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(target2, dummyDataFile, "English2");
        }


        [TestMethod]
        public void AvailableDictionaries_ReturnsCorrectDictionaryNames()
        {
            var expected = new List<string> { "English1", "English2" };
            var manager = new ExternalDictionaryManager(_dbf);
            var result = manager.AvailableDictionaries();
            CollectionAssert.AreEqual(expected, result);
        }


        [TestMethod]
        public void GetDictionary_ReturnsDictionaryIfExists()
        {
            string expectedName = "English1";

            var manager = new ExternalDictionaryManager(_dbf);

            ExternalDictionary result = manager.GetDictionary(expectedName);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedName, result.Source);
        }

        [TestMethod]
        public void GetDictionary_ReturnsNullIfDictionaryDoesNotExist()
        {
            string name = "NonExistingDict";

            var manager = new ExternalDictionaryManager(_dbf);

            ExternalDictionary result = manager.GetDictionary(name);

            Assert.IsNull(result);
        }

       
        [TestMethod]
        public void AddNewDictionaryFromCSV_AddsDictionarySuccessfully()
        {
            string name = "NewDict";
            string csvFileLocation = CreateMockCSVFile();

            var manager = new ExternalDictionaryManager(_dbf);

            manager.AddNewDictionaryFromCSV(csvFileLocation, name);

            var dictionaries = manager.AvailableDictionaries();
            Assert.IsTrue(dictionaries.Contains(name));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void AddNewDictionaryFromCSV_ThrowsExceptionForDuplicateName()
        {
            string name = "English1"; // Existing name
            string csvFileLocation = CreateMockCSVFile();

            var manager = new ExternalDictionaryManager(_dbf);

            manager.AddNewDictionaryFromCSV(csvFileLocation, name);
        }


        [TestMethod]
        public void VerifyIntegrityWith_PassesForValidConfig()
        {
            var manager = new ExternalDictionaryManager(_dbf);
            manager.VerifyIntegrity(manager.GetDictionary("English1"));
        }

        [TestCleanup]
        public void Cleanup()
        {
            using (var _db = new LinguineDbContext(ConnectionString))
            {
                _db.Database.EnsureDeleted(); // use this way as File method doesn't work
            }

            if (File.Exists("tmp.db"))
            {
                throw new Exception();
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
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during cleanup: " + ex.Message);
                // You might want to log this exception or handle it as needed
            }
        }

    }
}

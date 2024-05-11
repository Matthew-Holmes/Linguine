using Infrastructure;
using LearningStore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace LearningStoreTests
{
    [TestClass]
    public class ExternalDictionaryTests
    {
        private string _databaseFilePath = "testExternalDictionary.db";
        private string _connectionString;
        private ExternalDictionaryContext _context;
        private ExternalDictionary _dictionary;

        [TestInitialize]
        public void SetUp()
        {
            // Create a unique file name for the temporary database
            _connectionString = $"Data Source={_databaseFilePath};";

            // Create the database and schema
            _context = new ExternalDictionaryContext(_connectionString);
            _context.Database.EnsureCreated();

            // Add test data
            _context.DictionaryDefinitions.Add(new DictionaryDefinition { Word = "TestWord", Definition = "TestDefinition" });
            _context.DictionaryDefinitions.Add(new DictionaryDefinition { Word = "TestWordManyDef", Definition = "TestDefinition001" });
            _context.DictionaryDefinitions.Add(new DictionaryDefinition { Word = "TestWordManyDef", Definition = "TestDefinition002" });
            _context.SaveChanges();
        }

        [TestMethod]
        public void TryGetDefinitions_ExistingWord_ReturnsDefinition()
        {
            _dictionary = new ExternalDictionary(LanguageCode.eng, "TestDictionary", _connectionString);
            var result = _dictionary.TryGetDefinition("TestWord");

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual("TestDefinition", result[0].Definition);
        }

        [TestMethod]
        public void TryGetDefinitions_ExistingWord_ReturnsDefinitions()
        {
            _dictionary = new ExternalDictionary(LanguageCode.eng, "TestDictionary", _connectionString);
            var result = _dictionary.TryGetDefinition("TestWordManyDef");

            Assert.AreEqual("TestDefinition001", result[0].Definition);
            Assert.AreEqual("TestDefinition002", result[1].Definition);
            Assert.AreEqual(result.Count, 2);
        }

        [TestMethod]
        public void TryGetDefinitions_NonexistingWord_ReturnsEmptyList()
        {
            _dictionary = new ExternalDictionary(LanguageCode.eng, "TestDictionary", _connectionString);
            var result = _dictionary.TryGetDefinition("NoTestWord");

            Assert.AreEqual(result.Count, 0);
        }

        [TestMethod]
        public void Contains_WordExists_ReturnsTrue()
        {
            _dictionary = new ExternalDictionary(LanguageCode.eng, "TestDictionary", _connectionString);
            var result = _dictionary.Contains("TestWord");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Contains_WordNotExists_ReturnsFalse()
        {
            _dictionary = new ExternalDictionary(LanguageCode.eng, "TestDictionary", _connectionString);
            var result = _dictionary.Contains("NoTestWord");

            Assert.IsFalse(result);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Constructor_InvalidConnectionString_ThrowsException()
        {
            var invalidConnectionString = "Data Source=non_existent.db;";
            _dictionary = new ExternalDictionary(LanguageCode.eng, "TestDictionary", invalidConnectionString);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _context.Database.EnsureDeleted(); // use this way as File method doesn't work
            _dictionary?.Dispose();
            _context?.Dispose();

            _dictionary = null;
            _context = null;

            if (File.Exists(_databaseFilePath))
            {
                throw new Exception();
            }
        }
    }
}

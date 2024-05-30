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
        private const string ConnectionString = $"Data Source=tmp.db;";
        private LinguineDbContext _db;
        //private const string ConnectionString = $"Data Source=:memory:";


        [TestInitialize]
        public void SetUp()
        {
            _db?.Database.EnsureDeleted(); // use this way as File method doesn't work

            _db?.Dispose();

            if (File.Exists("tmp.db"))
            {
                throw new Exception();
            }

            _db = new LinguineDbContext(ConnectionString);
            _db.Database.EnsureCreated();
            _db.DictionaryDefinitions.Add(new DictionaryDefinition { Word = "TestWord", Definition = "TestDefinition", Source="demo" });
            _db.DictionaryDefinitions.Add(new DictionaryDefinition { Word = "TestWordManyDef", Definition = "TestDefinition001", Source = "demo" });
            _db.DictionaryDefinitions.Add(new DictionaryDefinition { Word = "TestWordManyDef", Definition = "TestDefinition002", Source = "demo" });
            _db.SaveChanges();
        }

        [TestMethod]
        public void TryGetDefinitions_ExistingWord_ReturnsDefinition()
        {
            ExternalDictionary _dictionary = new ExternalDictionary("demo", _db);
            var result = _dictionary.TryGetDefinition("TestWord");

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual("TestDefinition", result[0].Definition);
        }

        [TestMethod]
        public void TryGetDefinitions_ExistingWord_ReturnsDefinitions()
        {
            ExternalDictionary _dictionary = new ExternalDictionary("demo", _db);
            var result = _dictionary.TryGetDefinition("TestWordManyDef");

            Assert.AreEqual("TestDefinition001", result[0].Definition);
            Assert.AreEqual("TestDefinition002", result[1].Definition);
            Assert.AreEqual(result.Count, 2);
        }

        [TestMethod]
        public void TryGetDefinitions_NonexistingWord_ReturnsEmptyList()
        {
            ExternalDictionary _dictionary = new ExternalDictionary("demo", _db);
            var result = _dictionary.TryGetDefinition("NoTestWord");

            Assert.AreEqual(result.Count, 0);
        }

        [TestMethod]
        public void Contains_WordExists_ReturnsTrue()
        {
            ExternalDictionary _dictionary = new ExternalDictionary("demo", _db);
            var result = _dictionary.Contains("TestWord");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Contains_WordNotExists_ReturnsFalse()
        {
            ExternalDictionary _dictionary = new ExternalDictionary("demo", _db);
            var result = _dictionary.Contains("NoTestWord");

            Assert.IsFalse(result);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _db.Database.EnsureDeleted(); // use this way as File method doesn't work
           
            _db.Dispose();

            if (File.Exists("tmp.db"))
            {
                throw new Exception();
            }
        }
    }
}

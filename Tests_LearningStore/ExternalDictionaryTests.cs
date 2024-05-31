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

        [TestMethod]
        public void Add_ValidDefinition_ReturnsTrue()
        {
            var definition = new DictionaryDefinition
            {
                Word = "test",
                Definition = "A procedure intended to establish the quality, performance, or reliability of something.",
                Source = "demo"
            };

            ExternalDictionary _dictionary = new ExternalDictionary("demo", _db);

            var result = _dictionary.Add(definition);

            Assert.IsTrue(result);
            Assert.AreEqual(_dictionary.TryGetDefinition("test").FirstOrDefault().Definition,
                "A procedure intended to establish the quality, performance, or reliability of something.");
        }


        [TestMethod]
        public void Add_InvalidSourceDefinition_ReturnsFalse()
        {
            var definition = new DictionaryDefinition
            {
                Word = "test",
                Definition = "A procedure intended to establish the quality, performance, or reliability of something.",
                Source = "InvalidSource"
            };

            ExternalDictionary _dictionary = new ExternalDictionary("demo", _db);

            var result = _dictionary.Add(definition);

            Assert.IsFalse(result);
            Assert.AreEqual(0, _dictionary.TryGetDefinition("test").Count());
        }

        [TestMethod]
        public void Add_ValidDefinitionsList_ReturnsTrue()
        {
            var definitions = new List<DictionaryDefinition>
            {
                new DictionaryDefinition
                {
                    Word = "test1",
                    Definition = "Definition1",
                    Source = "demo"
                },
                new DictionaryDefinition
                {
                    Word = "test2",
                    Definition = "Definition2",
                    Source = "demo"
                }
            };

            ExternalDictionary _dictionary = new ExternalDictionary("demo", _db);


            var result = _dictionary.Add(definitions);

            Assert.IsTrue(result);
            Assert.AreEqual(_dictionary.TryGetDefinition("test1").FirstOrDefault().Definition, "Definition1");
            Assert.AreEqual(_dictionary.TryGetDefinition("test2").FirstOrDefault().Definition, "Definition2");
        }

        [TestMethod]
        public void Add_InvalidSourceDefinitionsList_ReturnsFalse()
        {
            var definitions = new List<DictionaryDefinition>
            {
                new DictionaryDefinition
                {
                    Word = "test1",
                    Definition = "Definition1",
                    Source = "demo"
                },
                new DictionaryDefinition
                {
                    Word = "test2",
                    Definition = "Definition2",
                    Source = "InvalidSource"
                }
            };

            ExternalDictionary _dictionary = new ExternalDictionary("demo", _db);

            var result = _dictionary.Add(definitions);

            Assert.IsFalse(result);
            Assert.AreEqual(0, _dictionary.TryGetDefinition("test1").Count());
            Assert.AreEqual(0, _dictionary.TryGetDefinition("test2").Count());
        }

        [TestMethod]
        public void DuplicateDefinitions_NoDuplicates_ReturnsFalse()
        {
            var definitions = new List<DictionaryDefinition>
            {
                new DictionaryDefinition { Word = "word1", Definition = "definition1", Source = "demo" },
                new DictionaryDefinition { Word = "word2", Definition = "definition2", Source = "demo" }
            };

            ExternalDictionary _dictionary = new ExternalDictionary("demo", _db);
            _dictionary.Add(definitions);

            var result = _dictionary.DuplicateDefinitions();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DuplicateDefinitions_HasDuplicates_ReturnsTrue()
        {
            var definitions = new List<DictionaryDefinition>
            {
                new DictionaryDefinition { Word = "word1", Definition = "definition1", Source = "demo" },
                new DictionaryDefinition { Word = "word1", Definition = "definition1", Source = "demo" }
            };

            ExternalDictionary _dictionary = new ExternalDictionary("demo", _db);
            _dictionary.Add(definitions);

            var result = _dictionary.DuplicateDefinitions();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DuplicateDefinitions_DifferentSources_ReturnsFalse()
        {

            var definitions = new List<DictionaryDefinition>
            {
                new DictionaryDefinition { Word = "word1", Definition = "definition1", Source = "demoA" },
                new DictionaryDefinition { Word = "word1", Definition = "definition1", Source = "demoB" }
            };

            ExternalDictionary _dictionaryA = new ExternalDictionary("demoA", _db);
            ExternalDictionary _dictionaryB = new ExternalDictionary("demoB", _db);
            _dictionaryA.Add(definitions[0]);
            _dictionaryB.Add(definitions[1]);

            var resultA = _dictionaryA.DuplicateDefinitions();
            Assert.IsFalse(resultA);

            var resultB = _dictionaryB.DuplicateDefinitions();
            Assert.IsFalse(resultB);
        }

        [TestMethod]
        public void DuplicateDefinitions_DuplicateWordsDifferentDefinitions_ReturnsFalse()
        {
            var definitions = new List<DictionaryDefinition>
            {
                new DictionaryDefinition { Word = "word1", Definition = "definition1", Source = "demo" },
                new DictionaryDefinition { Word = "word1", Definition = "definition2", Source = "demo" }
            };

            ExternalDictionary _dictionary = new ExternalDictionary("demo", _db);
            _dictionary.Add(definitions);

            var result = _dictionary.DuplicateDefinitions();

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

﻿using Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Tests_Infrastructure
{
    [TestClass]
    public class ExternalDictionaryTests
    {
        private const string ConnectionString = $"Data Source=tmp.db;";
        private LinguineDbContextFactory _dbf;
        //private const string ConnectionString = $"Data Source=:memory:";


        [TestInitialize]
        public void SetUp()
        {
            _dbf = new LinguineDbContextFactory(ConnectionString);

            var oldContext = _dbf?.CreateDbContext();
            oldContext?.Database.EnsureDeleted(); // use this way as File method doesn't work
            oldContext?.Dispose();

            if (File.Exists("tmp.db"))
            {
                throw new Exception();
            }

            var context = _dbf.CreateDbContext();
            context.Database.EnsureCreated();
            context.DictionaryDefinitions.Add(new DictionaryDefinition { Word = "TestWord", Definition = "TestDefinition", Source="demo" });
            context.DictionaryDefinitions.Add(new DictionaryDefinition { Word = "TestWordManyDef", Definition = "TestDefinition001", Source = "demo" });
            context.DictionaryDefinitions.Add(new DictionaryDefinition { Word = "TestWordManyDef", Definition = "TestDefinition002", Source = "demo" });
            context.SaveChanges();
            context.Dispose();
        }

        [TestMethod]
        public void TryGetDefinitions_ExistingWord_ReturnsDefinition()
        {
            ExternalDictionary _dictionary = new ExternalDictionary("demo", _dbf);
            var result = _dictionary.TryGetDefinition("TestWord");

            Assert.IsNotNull(result);
            Assert.AreEqual(result.Count, 1);
            Assert.AreEqual("TestDefinition", result[0].Definition);
        }

        [TestMethod]
        public void TryGetDefinitions_ExistingWord_ReturnsDefinitions()
        {
            ExternalDictionary _dictionary = new ExternalDictionary("demo", _dbf);
            var result = _dictionary.TryGetDefinition("TestWordManyDef");

            Assert.AreEqual("TestDefinition001", result[0].Definition);
            Assert.AreEqual("TestDefinition002", result[1].Definition);
            Assert.AreEqual(result.Count, 2);
        }

        [TestMethod]
        public void TryGetDefinitions_NonexistingWord_ReturnsEmptyList()
        {
            ExternalDictionary _dictionary = new ExternalDictionary("demo", _dbf);
            var result = _dictionary.TryGetDefinition("NoTestWord");

            Assert.AreEqual(result.Count, 0);
        }

        [TestMethod]
        public void Contains_WordExists_ReturnsTrue()
        {
            ExternalDictionary _dictionary = new ExternalDictionary("demo", _dbf);
            var result = _dictionary.Contains("TestWord");

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Contains_WordNotExists_ReturnsFalse()
        {
            ExternalDictionary _dictionary = new ExternalDictionary("demo", _dbf);
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

            ExternalDictionary _dictionary = new ExternalDictionary("demo", _dbf);

            using var context = _dbf.CreateDbContext();
            var result = _dictionary.Add(definition, context);

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

            ExternalDictionary _dictionary = new ExternalDictionary("demo", _dbf);

            using var context = _dbf.CreateDbContext();
            var result = _dictionary.Add(definition, context);

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

            ExternalDictionary _dictionary = new ExternalDictionary("demo", _dbf);


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

            ExternalDictionary _dictionary = new ExternalDictionary("demo", _dbf);

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

            ExternalDictionary _dictionary = new ExternalDictionary("demo", _dbf);
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

            ExternalDictionary _dictionary = new ExternalDictionary("demo", _dbf);
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

            ExternalDictionary _dictionaryA = new ExternalDictionary("demoA", _dbf);
            ExternalDictionary _dictionaryB = new ExternalDictionary("demoB", _dbf);
            using var context = _dbf.CreateDbContext();
            _dictionaryA.Add(definitions[0], context);
            _dictionaryB.Add(definitions[1], context);

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

            ExternalDictionary _dictionary = new ExternalDictionary("demo", _dbf);
            _dictionary.Add(definitions);

            var result = _dictionary.DuplicateDefinitions();

            Assert.IsFalse(result);
        }

        [TestCleanup]
        public void CleanUp()
        {
            var oldContext = _dbf?.CreateDbContext();
            oldContext?.Database?.EnsureDeleted(); // use this way as File method doesn't work
           
            oldContext?.Dispose();

            if (File.Exists("tmp.db"))
            {
                throw new Exception();
            }
        }
    }
}

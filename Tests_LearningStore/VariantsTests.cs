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
    public class VariantsTests
    {
        private const string ConnectionString = $"Data Source=tmp.db;";
        private LinguineDbContext _db;
        private Variants _variants;


        [TestInitialize]
        public void SetUp()
        {
            using (var _db = new LinguineDbContext(ConnectionString))
            {
                _db.Database.EnsureDeleted();
            }

            if (File.Exists("tmp.db"))
            {
                throw new Exception();
            }

            _db = new LinguineDbContext(ConnectionString);
            _db.Database.EnsureCreated();


            // Add test data
           _db.Variants.Add(new VariantRoot { Variant = "variant1", Root = "root1", Source = "variants1" });
           _db.Variants.Add(new VariantRoot { Variant = "variant2", Root = "root1", Source = "variants1" });
           _db.Variants.Add(new VariantRoot { Variant = "variant3", Root = "root2", Source = "variants1" });
           _db.Variants.Add(new VariantRoot { Variant = "variant1", Root = "root3", Source = "variants1" });
           _db.SaveChanges();

            _variants = new Variants("variants1", _db);
        }

        [TestMethod]
        public void GetVariants_WithExistingRoot_ReturnsVariants()
        {
            var result = _variants.GetVariants("root1").ToList();
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains("variant1"));
            Assert.IsTrue(result.Contains("variant2"));
        }

        [TestMethod]
        public void GetVariants_WithNonExistingRoot_ReturnsEmpty()
        {
            var result = _variants.GetVariants("nonexistent").ToList();
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetRoots_WithExistingVariant_ReturnsRoots()
        {

            var result = _variants.GetRoots("variant1").ToList();
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains("root1"));
            Assert.IsTrue(result.Contains("root3"));
        }

        [TestMethod]
        public void GetRoots_WithNonExistingVariant_ReturnsEmpty()
        {
            var result = _variants.GetRoots("nonexistent").ToList();
            Assert.AreEqual(0, result.Count);
        }


        [TestCleanup]
        public void CleanUp()
        {
            using (var _db = new LinguineDbContext(ConnectionString))
            {
                _db.Database.EnsureDeleted();
            }

            if (File.Exists("tmp.db"))
            {
                throw new Exception();
            }
        }
    }
}

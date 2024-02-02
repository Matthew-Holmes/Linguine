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
        private string _databaseFilePath = "testVariants.db";
        private string _connectionString;
        private VariantsContext _context;
        private Variants _variants;

        [TestInitialize]
        public void SetUp()
        {
            // Create a unique file name for the temporary database
            _connectionString = $"Data Source={_databaseFilePath};";

            // Create the database and schema
            _context = new VariantsContext(_connectionString);
            _context.Database.EnsureCreated();

            // Add test data
            _context.Variants.Add(new VariantRoot { Variant = "variant1", Root = "root1" });
            _context.Variants.Add(new VariantRoot { Variant = "variant2", Root = "root1" });
            _context.Variants.Add(new VariantRoot { Variant = "variant3", Root = "root2" });
            _context.Variants.Add(new VariantRoot { Variant = "variant1", Root = "root3" });
            _context.SaveChanges();

            
            _variants = new Variants(LanguageCode.eng, "test", _connectionString);
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


        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Constructor_InvalidConnectionString_ThrowsException()
        {
            var invalidConnectionString = "Data Source=non_existent.db;";
            _variants = new Variants(LanguageCode.eng, "TestDictionary", invalidConnectionString);
        }

        [TestCleanup]
        public void CleanUp()
        {
            _context.Database.EnsureDeleted(); // use this way as File method doesn't work
            _variants?.Dispose();
            _context?.Dispose();

            _variants = null;
            _context = null;

            if (File.Exists(_databaseFilePath))
            {
                throw new Exception();
            }
        }
    }
}

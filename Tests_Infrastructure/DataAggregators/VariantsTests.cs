using Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace Tests_Infrastructure
{
    [TestClass]
    public class VariantsTests
    {
        private const string ConnectionString = $"Data Source=tmp.db;";
        private LinguineDbContextFactory _dbf;
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

            _dbf = new LinguineDbContextFactory(ConnectionString);

            using var context = _dbf.CreateDbContext();

            context.Database.EnsureCreated();


            // Add test data
           context.Variants.Add(new VariantRoot { Variant = "variant1", Root = "root1", Source = "variants1" });
           context.Variants.Add(new VariantRoot { Variant = "variant2", Root = "root1", Source = "variants1" });
           context.Variants.Add(new VariantRoot { Variant = "variant3", Root = "root2", Source = "variants1" });
           context.Variants.Add(new VariantRoot { Variant = "variant1", Root = "root3", Source = "variants1" });
           context.SaveChanges();

            context.Dispose();
            _variants = new Variants("variants1", _dbf);

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
        public void Add_ValidVariantNewRoot_ReturnsTrue()
        {
            var variantRoot = new VariantRoot
            {
                Root = "root4",
                Variant = "variant4",
                Source = "variants1"
            };
            using var context = _dbf.CreateDbContext();
            var result = _variants.Add(variantRoot, context);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Add_ValidVariantNewRoot_Adds()
        {
            var variantRoot = new VariantRoot
            {
                Root = "root4",
                Variant = "variant4",
                Source = "variants1"
            };

            using var context = _dbf.CreateDbContext();
            var result = _variants.Add(variantRoot, context);

            Assert.IsTrue(result);

            Assert.AreEqual(_variants.GetRoots("variant4").FirstOrDefault(), "root4");
        }

        [TestMethod]
        public void Add_ValidVariantExistingRoot_ReturnsTrue()
        {
            var variantRoot = new VariantRoot
            {
                Root = "root1",
                Variant = "variant4",
                Source = "variants1"
            };

            using var context = _dbf.CreateDbContext();
            var result = _variants.Add(variantRoot, context);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void Add_ValidVariantExistingRoot_Adds()
        {
            var variantRoot = new VariantRoot
            {
                Root = "root1",
                Variant = "variant4",
                Source = "variants1"
            };

            using var context = _dbf.CreateDbContext();
            var result = _variants.Add(variantRoot, context);

            Assert.IsTrue(result);

            Assert.IsTrue(_variants.GetRoots("variant4").Contains("root1"));
        }

        [TestMethod]
        public void Add_InvalidSourceVariantRoot_ReturnsFalse()
        {
            var variantRoot = new VariantRoot
            {
                Root = "root4",
                Variant = "variant4",
                Source = "InvalidSource"
            };

            using var context = _dbf.CreateDbContext();
            var result = _variants.Add(variantRoot, context);

            Assert.IsFalse(result);
            Assert.IsFalse(_variants.GetRoots("variant4").Any());
        }

        [TestMethod]
        public void Add_ValidVariantRootsList_ReturnsTrue()
        {
            var variantRoots = new List<VariantRoot>
            {
                new VariantRoot { Root = "root4", Variant = "variant4", Source = "variants1" },
                new VariantRoot { Root = "root4", Variant = "variant5", Source = "variants1" }
            };

            var result = _variants.Add(variantRoots);

            Assert.IsTrue(result);
            Assert.AreEqual(2, _variants.GetVariants("root4").Count());
            Assert.IsTrue(_variants.GetVariants("root4").Contains("variant4"));
            Assert.IsTrue(_variants.GetVariants("root4").Contains("variant5"));
        }

        [TestMethod]
        public void Add_InvalidSourceVariantRootsList_ReturnsFalse()
        {
            var variantRoots = new List<VariantRoot>
            {
                new VariantRoot { Root = "root4", Variant = "variant4", Source = "variants1"},
                new VariantRoot { Root = "root4", Variant = "variant5", Source = "InvalidSource" }
            };

            var result = _variants.Add(variantRoots);

            Assert.IsFalse(result);
            Assert.IsFalse(_variants.GetVariants("root4").Contains("variant4"));
            Assert.IsFalse(_variants.GetVariants("root4").Contains("variant5"));
        }

        [TestMethod]
        public void DuplicateEntries_NoDuplicates_ReturnsFalse()
        {
            var result = _variants.DuplicateEntries();

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DuplicateEntries_HasDuplicates_ReturnsTrue()
        {
            var variantRoots = new List<VariantRoot>
            {
                new VariantRoot { Root = "root1", Variant = "variant1", Source = "variants1" },
                new VariantRoot { Root = "root1", Variant = "variant1", Source = "variants1" }
            };

            _variants.Add(variantRoots);

            var result = _variants.DuplicateEntries();

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DuplicateEntries_DifferentSources_ReturnsFalse()
        {
            var variantRoots = new List<VariantRoot>
            {
                new VariantRoot { Root = "root1", Variant = "variant1", Source = "variants2" },
                new VariantRoot { Root = "root1", Variant = "variant2", Source = "variants2" }
            };

            Variants variants2 = new Variants("variants2", _dbf);


            var tmp = variants2.Add(variantRoots);

            Assert.IsTrue(tmp);

            var result2 = variants2.DuplicateEntries();
            var result = _variants.DuplicateEntries();

            Assert.IsFalse(result2);
            Assert.IsFalse(result);
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

using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.ObjectModel;

namespace Tests_Infrastructure
{

    [TestClass]
    public class LinguineDataHandlerTests
    {
        private const string ConnectionString = $"Data Source=tmp.db;";
        //private const string ConnectionString = $"Data Source=:memory:";

        private void SeedData()
        {
            using (var context = new LinguineContext(ConnectionString))
            {
                context.Database.EnsureCreated();
            }
        }

        [TestMethod]
        public void CanInstantiateLinguineDbContext()
        {
            using (var context = new LinguineContext(ConnectionString))
            {
                Assert.IsNotNull(context);
            }
        }

        [TestMethod]
        public void CanAddAndRetrieveDictionaryDefinition()
        {
            SeedData();

            using (var context = new LinguineContext(ConnectionString))
            {
                context.DictionaryDefinitions.Add(new DictionaryDefinition { DatabasePrimaryKey = 1, Word = "word", Definition = "def", Source = "dict", ID = 1});
                context.SaveChanges();
            }

            using (var context = new LinguineContext(ConnectionString))
            {
                var entry = context.DictionaryDefinitions.Find(1);
                Assert.IsNotNull(entry);
                Assert.AreEqual(1, entry.DatabasePrimaryKey);
            }
        }

        [TestMethod]
        public void CanAddAndRetrieveVariantRoot()
        {
            SeedData();

            using (var context = new LinguineContext(ConnectionString))
            {
                context.Variants.Add(new VariantRoot { DatabasePrimaryKey = 1, Variant = "Can't", Root = "can not", Source = "roots" });
                context.SaveChanges();
            }

            using (var context = new LinguineContext(ConnectionString))
            {
                var entry = context.Variants.Find(1);
                Assert.IsNotNull(entry);
                Assert.AreEqual(1, entry.DatabasePrimaryKey);
            }
        }

        [TestMethod]
        public void OnModelCreating_ConfiguresModelCorrectly()
        {
            using (var context = new LinguineContext(ConnectionString))
            {
                var model = context.Model;

                var dictionaryDefinitionEntity = model.FindEntityType(typeof(DictionaryDefinition));
                Assert.IsNotNull(dictionaryDefinitionEntity);
                Assert.IsTrue(dictionaryDefinitionEntity.FindPrimaryKey().Properties
                    .Any(p => p.Name == nameof(DictionaryDefinition.DatabasePrimaryKey)));

                var variantRootEntity = model.FindEntityType(typeof(VariantRoot));
                Assert.IsNotNull(variantRootEntity);
                Assert.IsTrue(variantRootEntity.FindPrimaryKey().Properties
                    .Any(p => p.Name == nameof(VariantRoot.DatabasePrimaryKey)));
            }
        }

        [TestInitialize]
        [TestCleanup]
        public void CleanUp()
        {
            using (var _db = new LinguineContext(ConnectionString))
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


using Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Tests_Infrastructure
{
    [TestClass]
    public class VariantsManagerTests
    {
        private const string ConnectionString = $"Data Source=tmp.db;";
        private LinguineDbContext _db;

        private string dummyDataFile;

        [TestInitialize]
        public void TestInitialize()
        {
            _db?.Database.EnsureDeleted();
            _db?.Dispose();

            if (File.Exists("tmp.db"))
            {
                throw new Exception();
            }

            _db = new LinguineDbContext(ConnectionString);
            _db.Database.EnsureCreated();

            InitializeDummyDatabases();
            dummyDataFile = Path.Combine("dummyVariantsData.csv");
        }


        private static string CreateMockCSVFile()
        {
            string filePath = Path.Combine("dummyVariantsData.csv");
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("Variant,Root"); // CSV headers
            csvContent.AppendLine("variant1,root1");
            csvContent.AppendLine("variant2,root1");
            csvContent.AppendLine("variant3,root2");

            File.WriteAllText(filePath, csvContent.ToString(), Encoding.Unicode);

            return filePath;
        }

        private void InitializeDummyDatabases()
        {
            String dummyData = CreateMockCSVFile();

            var variants1 = new Variants("EnglishVariants1", _db);
            var variants2 = new Variants("EnglishVariants2", _db);

            VariantsCSVParser.ParseVariantsFromCSVToSQLiteAndSave(variants1, dummyData, "EnglishVariants1");
            VariantsCSVParser.ParseVariantsFromCSVToSQLiteAndSave(variants2, dummyData, "EnglishVariants2");
        }

        [TestMethod]
        public void AvailableVariants_ReturnsCorrectVariantNames()
        {
            var expected = new List<string> { "EnglishVariants1", "EnglishVariants2" };

            var manager = new VariantsManager(_db);

            var result = manager.AvailableVariantsSources();
            CollectionAssert.AreEqual(expected, result);
        }


        [TestMethod]
        public void GetVariants_ReturnsVariantsIfExists()
        {
            string expectedName = "EnglishVariants1";

            var manager = new VariantsManager(_db);

            var result = manager.GetVariantsSource(expectedName);

            Assert.IsNotNull(result);
            Assert.AreEqual(expectedName, result.Source);
        }

        [TestMethod]
        public void GetVariants_ReturnsNullIfVariantsDoesNotExist()
        {
            string name = "NonExistingVariants";
            var manager = new VariantsManager(_db);

            var result = manager.GetVariantsSource(name);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void AddNewVariantsFromCSV_AddsVariantsSuccessfully()
        {
            string name = "NewVariants";
            string csvFileLocation = CreateMockCSVFile();

            var manager = new VariantsManager(_db);

            manager.AddNewVariantsSourceFromCSV(csvFileLocation, name);

            var variants = manager.AvailableVariantsSources();
            Assert.IsTrue(variants.Contains(name));
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidDataException))]
        public void AddNewVariantsFromCSV_ThrowsExceptionForDuplicateName()
        {
            string name = "EnglishVariants1"; // Existing name
            string csvFileLocation = CreateMockCSVFile();

            var manager = new VariantsManager(_db);

            manager.AddNewVariantsSourceFromCSV(csvFileLocation, name);
        }

     
        [TestMethod]
        public void VerifyIntegrityWith_PassesForValidConfig()
        {
            var manager = new VariantsManager(_db);
            manager.VerifyIntegrity(manager.GetVariantsSource("EnglishVariants1"));
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

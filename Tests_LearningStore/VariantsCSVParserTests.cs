using Infrastructure;
using LearningStore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;
using System.IO;
using System.Text;

namespace LearningStoreTests
{
    [TestClass]
    public class VariantsCSVParserTests
    {
        private string _csvFilePath;
        private string _name;

        private const string ConnectionString = $"Data Source=tmp.db;";
        private LinguineDbContext _db;


        [TestInitialize]
        public void SetUp()
        {
            using (var _db = new LinguineDbContext(ConnectionString))
            {
                _db.Database.EnsureDeleted(); // use this way as File method doesn't work
            }

            if (File.Exists("tmp.db"))
            {
                throw new Exception();
            }

            _name = "testVariantsForCsv";

            // Create a mock CSV file with test data
            _csvFilePath = CreateMockCSVFile();

            _db = new LinguineDbContext(ConnectionString);
            _db.Database.EnsureCreated();
        }

        private string CreateMockCSVFile()
        {
            string filePath = Path.Combine("testVariantsData.csv");
            StringBuilder csvContent = new StringBuilder();
            csvContent.AppendLine("Variant,Root"); // CSV headers
            csvContent.AppendLine("variant1,root1");
            csvContent.AppendLine("variant2,root1");
            csvContent.AppendLine("variant3,root2");

            File.WriteAllText(filePath, csvContent.ToString(), Encoding.Unicode);

            return filePath;
        }

        [TestMethod]
        public void ParseVariantsFromCSV_ValidCSV_Runs()
        {
            Variants target = new Variants(_name, _db);

            VariantsCSVParser.ParseVariantsFromCSVToSQLiteAndSave(target, _csvFilePath, _name);
        }

        [TestMethod]
        public void ParseVariantsFromCSV_ValidCSV_CreatesDatabase()
        {

            Variants target = new Variants(_name, _db);

            VariantsCSVParser.ParseVariantsFromCSVToSQLiteAndSave(target, _csvFilePath, _name);

            // Check the first entry
            var variantEntry1 = target.GetRoots("variant1").FirstOrDefault();
            Assert.IsNotNull(variantEntry1, "Variant entry 1 should not be null");
            Assert.AreEqual("root1", variantEntry1, "Variant entry 1 root is incorrect");

            var variantEntry2 = target.GetRoots("variant2").FirstOrDefault();
            Assert.IsNotNull(variantEntry2, "Variant entry 2 should not be null");
            Assert.AreEqual("root1", variantEntry2, "Variant entry 2 root is incorrect");

            var variantEntry3 = target.GetRoots("variant3").FirstOrDefault();
            Assert.IsNotNull(variantEntry3, "Variant entry 3 should not be null");
            Assert.AreEqual("root2", variantEntry3, "Variant entry 3 root is incorrect");
        }


        [TestMethod]
        [ExpectedException(typeof(DataException))]
        public void ParseVariantsFromCSV_EmptyCSV_ThrowsException()
        {
            // Create an empty CSV file
            string emptyCsvFilePath = CreateEmptyCSVFile();

            Variants target = new Variants(_name, _db);

            VariantsCSVParser.ParseVariantsFromCSVToSQLiteAndSave(target, emptyCsvFilePath, _name);
        }

        private string CreateEmptyCSVFile()
        {
            string filePath = Path.Combine("emptyVariants.csv");
            File.WriteAllText(filePath, "Variant,Root", Encoding.Unicode);

            return filePath;
        }

        [TestCleanup]
        public void CleanUp()
        {
            // Delete the temporary CSV file
            if (File.Exists(_csvFilePath))
            {
                File.Delete(_csvFilePath);
            }

            // Create the database and schema

            using (var _db = new LinguineDbContext(ConnectionString))
            {
                _db.Database.EnsureDeleted(); // use this way as File method doesn't work
            }

            if (File.Exists("tmp.db"))
            {
                throw new Exception();
            }

            if (File.Exists("emptyVariants.csv"))
            {
                File.Delete("emptyVariants.csv");
            }
        }
    }
}

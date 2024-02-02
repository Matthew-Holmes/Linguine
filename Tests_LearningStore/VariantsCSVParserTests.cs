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
        private string _connectionString;
        private string _name;

        [TestInitialize]
        public void SetUp()
        {
            _name = "testVariantsForCsv";

            // Create a mock CSV file with test data
            _csvFilePath = CreateMockCSVFile();
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
            _connectionString = VariantsCSVParser.ParseVariantsFromCSVToSQLiteAndSave(_csvFilePath, LanguageCode.eng, _name);
        }

        [TestMethod]
        public void ParseVariantsFromCSV_ValidCSV_CreatesDatabase()
        {
            _connectionString = VariantsCSVParser.ParseVariantsFromCSVToSQLiteAndSave(_csvFilePath, LanguageCode.eng, _name);

            using (VariantsContext context = new VariantsContext(_connectionString))
            {
                // Check the first entry
                var variantEntry1 = context.Variants.FirstOrDefault(v => v.Variant == "variant1");
                Assert.IsNotNull(variantEntry1, "Variant entry 1 should not be null");
                Assert.AreEqual("root1", variantEntry1.Root, "Variant entry 1 root is incorrect");

                // Check the second entry
                var variantEntry2 = context.Variants.FirstOrDefault(v => v.Variant == "variant2");
                Assert.IsNotNull(variantEntry2, "Variant entry 2 should not be null");
                Assert.AreEqual("root1", variantEntry2.Root, "Variant entry 2 root is incorrect");

                // Check the third entry
                var variantEntry3 = context.Variants.FirstOrDefault(v => v.Variant == "variant3");
                Assert.IsNotNull(variantEntry3, "Variant entry 3 should not be null");
                Assert.AreEqual("root2", variantEntry3.Root, "Variant entry 3 root is incorrect");

                // Check the total number of entries in the database
                int totalEntries = context.Variants.Count();
                Assert.AreEqual(3, totalEntries, "The number of entries in the database is incorrect");
            }
        }


        [TestMethod]
        [ExpectedException(typeof(DataException))]
        public void ParseVariantsFromCSV_EmptyCSV_ThrowsException()
        {
            // Create an empty CSV file
            string emptyCsvFilePath = CreateEmptyCSVFile();

            _connectionString = VariantsCSVParser.ParseVariantsFromCSVToSQLiteAndSave(emptyCsvFilePath, LanguageCode.eng, _name);
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

            if (_connectionString is not null)
            {
                VariantsContext context = new VariantsContext(_connectionString);
                context.Database.EnsureDeleted();
                _connectionString = null;
            }

            if (File.Exists("emptyVariants.csv"))
            {
                File.Delete("emptyVariants.csv");
            }
        }
    }
}

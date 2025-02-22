using Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_Infrastructure
{
    [TestClass]
    public class ParsedDictionaryDefinitionManagerTests
    {
        private const string ConnectionString = $"Data Source=tmp.db;";
        private LinguineDbContextFactory _dbf;

        private DictionaryDefinition? def1 = null;
        private DictionaryDefinition? def2 = null; 

        [TestInitialize]
        public void TestInitialize()
        {
            using (var _db = new LinguineDbContext(ConnectionString))
            {
                _db.Database.EnsureDeleted(); // use this way as File method doesn't work
            }

            if (File.Exists("tmp.db"))
            {
                throw new Exception();
            }

            _dbf = new LinguineDbContextFactory(ConnectionString);

            var context = _dbf.CreateDbContext();

            context.Database.EnsureCreated();

            InitializeDummyData();

            context.Dispose();

        }

        private void InitializeDummyData()
        {
            def1 = new DictionaryDefinition
            {
                Word = "test",
                Source = "dummy",
                ID = 1,
                Definition = "a way to determine if some behaviour is correct"
            };

            def2 = new DictionaryDefinition
            {
                Word = "word",
                Source = "dummy",
                ID = 2,
                Definition = "a written task set to determine a students ability level"
            };

            DictionaryDefinition def3 = new DictionaryDefinition
            {
                Word = "word",
                Source = "dummy",
                ID = 3,
                Definition = "a unit of text"
            };


            using var context = _dbf.CreateDbContext();

            context.DictionaryDefinitions.Add(def1);
            context.DictionaryDefinitions.Add(def2);
            context.DictionaryDefinitions.Add(def3);


            ParsedDictionaryDefinition pdef1 = new ParsedDictionaryDefinition { CoreDefinition = def1, LearnerLevel = LearnerLevel.beginner, NativeLanguage = LanguageCode.eng, ParsedDefinition = "a way to check things" };

            ParsedDictionaryDefinition pdef2 = new ParsedDictionaryDefinition { CoreDefinition = def1, LearnerLevel = LearnerLevel.beginner, NativeLanguage = LanguageCode.eng, ParsedDefinition = "an exam" };

            ParsedDictionaryDefinition pdef3 = new ParsedDictionaryDefinition { CoreDefinition = def1, LearnerLevel = LearnerLevel.intermediate, NativeLanguage = LanguageCode.eng, ParsedDefinition = "a written exam for students" };

            ParsedDictionaryDefinition pdef4 = new ParsedDictionaryDefinition { CoreDefinition = def1, LearnerLevel = LearnerLevel.intermediate, NativeLanguage = LanguageCode.fra, ParsedDefinition = "un examen écrit pour les étudiants" };

            context.ParsedDictionaryDefinitions.Add(pdef1);
            context.ParsedDictionaryDefinitions.Add(pdef2);
            context.ParsedDictionaryDefinitions.Add(pdef3);
            context.ParsedDictionaryDefinitions.Add(pdef4);

            context.SaveChanges();

        }

        [TestMethod]
        public void GetParsedDictionaryDefinition_ReturnsExisting()
        {
            ParsedDictionaryDefinitionManager manager = new ParsedDictionaryDefinitionManager(_dbf);

            Assert.IsNotNull(def1);

            ParsedDictionaryDefinition? pdef = manager.GetParsedDictionaryDefinition(
                def1, LearnerLevel.beginner, LanguageCode.eng);

            Assert.IsNotNull(pdef);

            Assert.AreEqual(pdef.ParsedDefinition, "a way to check things");
        }

        [TestMethod]
        public void GetParsedDictionaryDefinition_ReturnsNullIfNonexisting()
        {
            ParsedDictionaryDefinitionManager manager = new ParsedDictionaryDefinitionManager(_dbf);

            Assert.IsNotNull(def1);

            ParsedDictionaryDefinition? pdef = manager.GetParsedDictionaryDefinition(
                def1, LearnerLevel.elementary, LanguageCode.eng);

            Assert.IsNull(pdef);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_ThrowsIfExistsAlready()
        {
            ParsedDictionaryDefinition pdef1_again = new ParsedDictionaryDefinition { CoreDefinition = def1, LearnerLevel = LearnerLevel.beginner, NativeLanguage = LanguageCode.eng, ParsedDefinition = "a method to check things" };

            ParsedDictionaryDefinitionManager manager = new ParsedDictionaryDefinitionManager(_dbf);

            using var context = _dbf.CreateDbContext();
            manager.Add(pdef1_again, context);
        }



        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Add_ThrowsUnknownDictionaryDefinition()
        {
            DictionaryDefinition def = new DictionaryDefinition
            {
                Word = "unknown",
                Definition = "not recorded, unseen previously",
                ID = 42
            };

            ParsedDictionaryDefinition pdef = new ParsedDictionaryDefinition { CoreDefinition = def, LearnerLevel = LearnerLevel.beginner, NativeLanguage = LanguageCode.eng, ParsedDefinition = "not known" };

            ParsedDictionaryDefinitionManager manager = new ParsedDictionaryDefinitionManager(_dbf);

            using var context = _dbf.CreateDbContext();
            manager.Add(pdef, context);
        }

        [TestMethod]
        public void Add_NoThrowIfNew()
        {
            ParsedDictionaryDefinition pdef1_again = new ParsedDictionaryDefinition { CoreDefinition = def1, LearnerLevel = LearnerLevel.elementary, NativeLanguage = LanguageCode.eng, ParsedDefinition = "a method to check things" };

            ParsedDictionaryDefinitionManager manager = new ParsedDictionaryDefinitionManager(_dbf);

            using var context = _dbf.CreateDbContext();
            manager.Add(pdef1_again, context);
        }

        [TestMethod]
        public void Add_CorrectlyAdded()
        {
            ParsedDictionaryDefinition pdef = new ParsedDictionaryDefinition { CoreDefinition = def1, LearnerLevel = LearnerLevel.elementary, NativeLanguage = LanguageCode.eng, ParsedDefinition = "a method to check things" };

            ParsedDictionaryDefinitionManager manager = new ParsedDictionaryDefinitionManager(_dbf);

            using var context = _dbf.CreateDbContext();
            manager.Add(pdef, context);

            ParsedDictionaryDefinition? found = manager.GetParsedDictionaryDefinition(def1, LearnerLevel.elementary, LanguageCode.eng);

            Assert.IsNotNull(found);    
        }


        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void AddSet_ThrowsIfOneAlreadyExist()
        {
            ParsedDictionaryDefinition pdef1_again = new ParsedDictionaryDefinition { CoreDefinition = def1, LearnerLevel = LearnerLevel.beginner, NativeLanguage = LanguageCode.eng, ParsedDefinition = "a method to check things" };

            ParsedDictionaryDefinition pdef = new ParsedDictionaryDefinition { CoreDefinition = def1, LearnerLevel = LearnerLevel.elementary, NativeLanguage = LanguageCode.eng, ParsedDefinition = "a method to check things" };

            HashSet<ParsedDictionaryDefinition> toAdd = new HashSet<ParsedDictionaryDefinition> { pdef1_again, pdef };

            ParsedDictionaryDefinitionManager manager = new ParsedDictionaryDefinitionManager(_dbf);

            manager.AddSet(toAdd);

        }

        [TestMethod]
        public void AddSet_NoThrowIfAllAreNew()
        {
            ParsedDictionaryDefinition pdef1 = new ParsedDictionaryDefinition { CoreDefinition = def1, LearnerLevel = LearnerLevel.elementary, NativeLanguage = LanguageCode.eng, ParsedDefinition = "a method to check things" };

            ParsedDictionaryDefinition pdef2 = new ParsedDictionaryDefinition { CoreDefinition = def1, LearnerLevel = LearnerLevel.native, NativeLanguage = LanguageCode.eng, ParsedDefinition = def1.Definition };

            HashSet<ParsedDictionaryDefinition> toAdd = new HashSet<ParsedDictionaryDefinition> { pdef1, pdef2 };

            ParsedDictionaryDefinitionManager manager = new ParsedDictionaryDefinitionManager(_dbf);

            manager.AddSet(toAdd);

        }

        [TestMethod]
        public void AddSet_AllAreAdded()
        {
            ParsedDictionaryDefinition pdef1 = new ParsedDictionaryDefinition { CoreDefinition = def1, LearnerLevel = LearnerLevel.elementary, NativeLanguage = LanguageCode.eng, ParsedDefinition = "a method to check things" };

            ParsedDictionaryDefinition pdef2 = new ParsedDictionaryDefinition { CoreDefinition = def1, LearnerLevel = LearnerLevel.native, NativeLanguage = LanguageCode.eng, ParsedDefinition = def1.Definition };

            HashSet<ParsedDictionaryDefinition> toAdd = new HashSet<ParsedDictionaryDefinition> { pdef1, pdef2 };

            ParsedDictionaryDefinitionManager manager = new ParsedDictionaryDefinitionManager(_dbf);

            manager.AddSet(toAdd);

            ParsedDictionaryDefinition? found1 = manager.GetParsedDictionaryDefinition(
                def1, LearnerLevel.intermediate, LanguageCode.eng);
            ParsedDictionaryDefinition? found2 = manager.GetParsedDictionaryDefinition(
                def1, LearnerLevel.native, LanguageCode.eng);

            Assert.IsNotNull(found1);
            Assert.IsNotNull(found2);
        }

        [TestMethod]
        public void FilterOutKnown_doesFiltering()
        {
            HashSet<DictionaryDefinition> defs = new HashSet<DictionaryDefinition> { def1, def2 };

            ParsedDictionaryDefinitionManager manager = new ParsedDictionaryDefinitionManager(_dbf);

            HashSet<DictionaryDefinition> filtered = manager.FilterOutKnown(defs, LearnerLevel.intermediate, LanguageCode.fra);

            Assert.AreEqual(filtered.Count, 1);
            Assert.AreEqual(filtered.First(), def2);
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
    }
}

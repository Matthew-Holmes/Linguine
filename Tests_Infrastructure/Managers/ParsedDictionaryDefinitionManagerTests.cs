using Infrastructure;
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
        private LinguineDataHandler _db;

        [TestInitialize]
        public void TestInitialize()
        {
            using (var _db = new LinguineDataHandler(ConnectionString))
            {
                _db.Database.EnsureDeleted(); // use this way as File method doesn't work
            }

            if (File.Exists("tmp.db"))
            {
                throw new Exception();
            }

            _db = new LinguineDataHandler(ConnectionString);
            _db.Database.EnsureCreated();

            InitializeDummyData();

        }

        private void InitializeDummyData()
        {
            DictionaryDefinition def1 = new DictionaryDefinition
            {
                Word = "test",
                Source = "dummy",
                ID = 1,
                Definition = "a way to determine if some behaviour is correct"
            };

            DictionaryDefinition def2 = new DictionaryDefinition
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


            _db.DictionaryDefinitions.Add(def1);
            _db.DictionaryDefinitions.Add(def2);
            _db.DictionaryDefinitions.Add(def3);


            ParsedDictionaryDefinition pdef1 = new ParsedDictionaryDefinition { CoreDefinition = def1, LearnerLevel = LearnerLevel.beginner, NativeLanguage = LanguageCode.eng, ParsedDefinition = "a way to check things" };

            ParsedDictionaryDefinition pdef2 = new ParsedDictionaryDefinition { CoreDefinition = def1, LearnerLevel = LearnerLevel.beginner, NativeLanguage = LanguageCode.eng, ParsedDefinition = "an exam" };

            ParsedDictionaryDefinition pdef3 = new ParsedDictionaryDefinition { CoreDefinition = def1, LearnerLevel = LearnerLevel.intermediate, NativeLanguage = LanguageCode.eng, ParsedDefinition = "a written exam for students" };

            ParsedDictionaryDefinition pdef4 = new ParsedDictionaryDefinition { CoreDefinition = def1, LearnerLevel = LearnerLevel.intermediate, NativeLanguage = LanguageCode.fra, ParsedDefinition = "un examen écrit pour les étudiants" };

            _db.ParsedDictionaryDefinitions.Add(pdef1);
            _db.ParsedDictionaryDefinitions.Add(pdef2);
            _db.ParsedDictionaryDefinitions.Add(pdef3);
            _db.ParsedDictionaryDefinitions.Add(pdef4);

            _db.SaveChanges();

        }




        [TestCleanup]
        public void Cleanup()
        {
            using (var _db = new LinguineDataHandler(ConnectionString))
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

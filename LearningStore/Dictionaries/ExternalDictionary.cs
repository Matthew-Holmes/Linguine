using Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LearningStore
{
    public class ExternalDictionary
    {
        public String Source { get; }

        private LinguineDbContext _db;

        public ExternalDictionary(String source, LinguineDbContext db)
        {
            Source = source;
            _db = db;   
        }

        public List<DictionaryDefinition> TryGetDefinition(String word)
        {
            return _db.DictionaryDefinitions.Where(def => def.Source == Source).Where(dd => dd.Word == word).ToList();
        }

        public bool Contains(String word)
        {
            return _db.DictionaryDefinitions.Where(def => def.Source == Source).Any(dd => dd.Word == word);
        }

        internal bool Add(DictionaryDefinition definition, bool save = true)
        {
            // TODO - tests
            if (definition.Source != Source)
            {
                return false;
            }

            _db.DictionaryDefinitions.Add(definition);

            if (save)
            {
                _db.SaveChanges();
            }

            return true;
        }

        internal bool Add(List<DictionaryDefinition> definitions)
        {   
            // TODO - tests
            if (definitions.Any(def => def.Source != Source))
            {
                return false;
            }

            foreach (var def in definitions)
            {
               Add(def, false);
            }

            _db.SaveChanges();

            return true;
        }

        public bool DuplicateDefinitions()
        {
            return _db.DictionaryDefinitions
                        .Where(def => def.Source == Source)
                        .GroupBy(p => new { p.Word, p.Definition })
                        .Where(p => p.Count() > 1)
                        .Any();
        }

    }

}

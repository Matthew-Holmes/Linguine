using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class ParsedDictionaryDefinitionManager : ManagerBase
    {
        public ParsedDictionaryDefinitionManager(LinguineDataHandler db) : base(db)
        {
        }

        public ParsedDictionaryDefinition? GetParsedDictionaryDefinition(
            DictionaryDefinition core, LearnerLevel level, LanguageCode nativeLanguage)
        {
            return _db.ParsedDictionaryDefinitions
                .Where(d => d.CoreDefinition == core)
                .Where(d => d.NativeLanguage == nativeLanguage)
                .Where(d => d.LearnerLevel == level)
                .FirstOrDefault();
        }

        public void Add(ParsedDictionaryDefinition pDef)
        {
            if (GetParsedDictionaryDefinition(pDef.CoreDefinition, pDef.LearnerLevel, pDef.NativeLanguage) != null)
            {
                throw new Exception("trying to add a parsed definition that already exists!");
            }
            _db.Add(pDef);
            _db.SaveChanges();
        }

        public void AddSet(HashSet<ParsedDictionaryDefinition> definitions)
        {
            foreach (var def in definitions)
            {
                Add(def);  
            }
            _db.SaveChanges(); 
        }

        public HashSet<DictionaryDefinition> FilterOutKnown(
            HashSet<DictionaryDefinition> definitions, LearnerLevel level, LanguageCode native)
        {
            HashSet<DictionaryDefinition> ret = new HashSet<DictionaryDefinition>();

            foreach (DictionaryDefinition def in definitions)
            {
                if (GetParsedDictionaryDefinition(def, level, native) is null)
                {
                    ret.Add(def);
                }
            }

            return ret;
        }
    }
}

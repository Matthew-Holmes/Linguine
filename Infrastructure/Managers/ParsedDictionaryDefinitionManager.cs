using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class ParsedDictionaryDefinitionManager : ManagerBase
    {
        public ParsedDictionaryDefinitionManager(String conn) : base(conn)
        {
        }

        public ParsedDictionaryDefinition? GetParsedDictionaryDefinition(
            DictionaryDefinition core, LearnerLevel level, LanguageCode nativeLanguage)
        {
            using LinguineContext lg = Linguine();
            return lg.ParsedDictionaryDefinitions
                .Where(d => d.CoreDefinition == core)
                .Where(d => d.NativeLanguage == nativeLanguage)
                .Where(d => d.LearnerLevel == level)
                .FirstOrDefault();
        }

        public void Add(ParsedDictionaryDefinition pDef, LinguineContext? lg)
        {
            if (GetParsedDictionaryDefinition(pDef.CoreDefinition, pDef.LearnerLevel, pDef.NativeLanguage) != null)
            {
                throw new ArgumentException("trying to add a parsed definition that already exists!");
            }

            if (lg is null)
            {
                using LinguineContext lg_tmp = Linguine();

                if (lg_tmp.DictionaryDefinitions.Contains(pDef.CoreDefinition) is false)
                {
                    throw new ArgumentException("core definition not in the database!");
                }

                lg_tmp.ParsedDictionaryDefinitions.Add(pDef);
                lg_tmp.SaveChanges();
            } 
            else
            {
                if (lg.DictionaryDefinitions.Contains(pDef.CoreDefinition) is false)
                {
                    throw new ArgumentException("core definition not in the database!");
                }

                lg.ParsedDictionaryDefinitions.Add(pDef);
            }
        }

        public void AddSet(HashSet<ParsedDictionaryDefinition> definitions)
        {
            using LinguineContext lg = Linguine();
            foreach (var def in definitions)
            {
                Add(def, lg);  
            }
            lg.SaveChanges(); 
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

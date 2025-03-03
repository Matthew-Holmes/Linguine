using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class ParsedDictionaryDefinitionManager : ManagerBase
    {
        public ParsedDictionaryDefinitionManager(LinguineDbContextFactory dbf) : base(dbf)
        {
        }

        public ParsedDictionaryDefinition? GetParsedDictionaryDefinition(
            DictionaryDefinition core, LearnerLevel level, LanguageCode nativeLanguage)
        {
            using var context = _dbf.CreateDbContext();
            return context.ParsedDictionaryDefinitions
                .Where(d => d.CoreDefinition == core)
                .Where(d => d.NativeLanguage == nativeLanguage)
                .Where(d => d.LearnerLevel == level)
                .FirstOrDefault();
        }

        public void Add(ParsedDictionaryDefinition pDef, LinguineDbContext context, bool save = true)
        {
            if (context.DictionaryDefinitions.Contains(pDef.CoreDefinition) is false)
            {
                throw new ArgumentException("core definition not in the database!");
            }

            if (GetParsedDictionaryDefinition(pDef.CoreDefinition, pDef.LearnerLevel, pDef.NativeLanguage) != null)
            {
                throw new ArgumentException("trying to add a parsed definition that already exists!");
            }

            context.Attach(pDef.CoreDefinition);
            context.Add(pDef);

            if (save == true)
            {
                context.SaveChanges();
            }
        }

        public void AddSet(HashSet<ParsedDictionaryDefinition> definitions)
        {
            using var context = _dbf.CreateDbContext();

            foreach (var def in definitions)
            {
                Add(def, context, false);
                context.SaveChanges(); // TODO - fix like in statement manager
                context.ChangeTracker.Clear();
            }

        }

        public HashSet<DictionaryDefinition> FilterOutKnown(
            HashSet<DictionaryDefinition> definitions, LearnerLevel level, LanguageCode native)
        {
            HashSet<DictionaryDefinition> ret = new HashSet<DictionaryDefinition>();

            HashSet<int> ids = new(); // mayber I need to add a hashcode method to definition to get this to work

            foreach (DictionaryDefinition def in definitions)
            {
                if (GetParsedDictionaryDefinition(def, level, native) is null)
                {
                    if (ids.Contains(def.ID))
                    {
                        /* do nothing */
                    }
                    else
                    {
                        ret.Add(def);
                        ids.Add(def.ID);
                    }
                }
            }

            return ret;
        }
    }
}

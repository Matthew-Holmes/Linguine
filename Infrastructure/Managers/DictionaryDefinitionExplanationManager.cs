using DataClasses;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class DictionaryDefinitionExplanationManager : DataManagerBase
    {
        public DictionaryDefinitionExplanationManager(LinguineReadonlyDbContextFactory dbf) : base(dbf)
        {
        }

        public void Add(DictionaryDefinitionExplanation expl, LinguineDbContext context, bool save = true)
        {
            if (context.DictionaryDefinitions.Contains(expl.CoreDefinition) is false)
            {
                throw new ArgumentException("core definition not in the database!");
            }

            if (GetDefinitionExplanation(expl.CoreDefinition.DatabasePrimaryKey, expl.NativeLanguage) != null)
            {
                throw new ArgumentException("trying to add a definition explanation that already exists!");
            }

            context.Attach(expl.CoreDefinition);
            context.Add(expl);

            if (save == true)
            {
                context.SaveChanges();
            }
        }

        public DictionaryDefinitionExplanation? GetDefinitionExplanation(
            int defKey,  LanguageCode nativeLanguage)
        {
            using var context = _dbf.CreateDbContext();
            return context.DefinitionExplanations
                       .Where(d => d.DictionaryDefinitionKey == defKey)
                       .Where(d => d.NativeLanguage == nativeLanguage)
                       .Include(d => d.CoreDefinition)
                       .FirstOrDefault();
        }

        public HashSet<DictionaryDefinition> FilterOutKnown(
           HashSet<DictionaryDefinition> definitions, LanguageCode native)
        {
            HashSet<DictionaryDefinition> ret = new HashSet<DictionaryDefinition>();

            HashSet<int> ids = new();

            foreach (DictionaryDefinition def in definitions)
            {
                if (GetDefinitionExplanation(def.DatabasePrimaryKey, native) is null)
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

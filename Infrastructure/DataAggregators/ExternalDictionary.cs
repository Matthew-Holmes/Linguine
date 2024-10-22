using Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Operations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Infrastructure
{
    public class ExternalDictionary : ManagerBase
    {
        public String Source { get; }


        public ExternalDictionary(String source, String conn) : base(conn)
        {
            Source = source;
        }

        public List<DictionaryDefinition> TryGetDefinition(String word)
        {
            using LinguineContext lg = Linguine();
            return lg.DictionaryDefinitions.Where(def => def.Source == Source).Where(dd => dd.Word == word).ToList();
        }

        public bool Contains(String word)
        {
            using LinguineContext lg = Linguine();
            return lg.DictionaryDefinitions.Where(def => def.Source == Source).Any(dd => dd.Word == word);
        }

        internal bool Add(DictionaryDefinition definition, LinguineContext? lg)
        {
            if (definition.Source != Source)
            {
                return false;
            }

            if (lg is null)
            {
                using LinguineContext lg_tmp = Linguine();
                lg_tmp.DictionaryDefinitions.Add(definition);
                lg_tmp.SaveChanges();
            }
            else
            {
                lg.DictionaryDefinitions.Add(definition);
            }

            return true;
        }

        internal bool Add(List<DictionaryDefinition> definitions)
        {   
            if (definitions.Any(def => def.Source != Source))
            {
                return false;
            }

            using LinguineContext lg = Linguine();
            foreach (var def in definitions)
            {
               Add(def, lg);
            }

            lg.SaveChanges();

            return true;
        }

        public bool DuplicateDefinitions()
        {
            using LinguineContext lg = Linguine();
            return lg.DictionaryDefinitions
                        .Where(def => def.Source == Source)
                        .GroupBy(p => new { p.Word, p.Definition })
                        .Where(p => p.Count() > 1)
                        .Any();
        }

    }

}

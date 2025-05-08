using DataClasses;

namespace Infrastructure
{
    public class ExternalDictionary
    {
        private LinguineDbContextFactory _dbf;

        public ExternalDictionary(LinguineDbContextFactory dbf)
        {
            _dbf = dbf;   
        }

        public DictionaryDefinition GetRandomDefinition()
        {
            using var context = _dbf.CreateDbContext();
            int minId = context.DictionaryDefinitions.Min(t => t.ID);
            int maxId = context.DictionaryDefinitions.Max(t => t.ID);

            Random rnd = new Random();
            int randomId = rnd.Next(minId, maxId + 1);

            DictionaryDefinition? ret = null;

            do
            {
                // will handle missing data
                randomId = rnd.Next(minId, maxId + 1);
                ret = context.DictionaryDefinitions.FirstOrDefault(t => t.ID == randomId);
            }
            while (ret == null);


            return ret;
        }

        public List<DictionaryDefinition> TryGetDefinition(String word)
        {
            using var context = _dbf.CreateDbContext();
            return context.DictionaryDefinitions.Where(dd => dd.Word == word).ToList();
        }

        public DictionaryDefinition? TryGetDefinitionByKey(int key)
        {
            using var context = _dbf.CreateDbContext();
            return context.DictionaryDefinitions.Where(def => def.DatabasePrimaryKey == key).SingleOrDefault();
        }

        public bool Contains(String word)
        {
            using var context = _dbf.CreateDbContext();
            return context.DictionaryDefinitions.Any(dd => dd.Word == word);
        }

        internal bool Add(DictionaryDefinition definition, LinguineDbContext context, bool save = true)
        {
            context.DictionaryDefinitions.Add(definition);

            if (save)
            {
                context.SaveChanges();
            }

            return true;
        }

        internal bool Add(List<DictionaryDefinition> definitions)
        {
            using var context = _dbf.CreateDbContext();

            foreach (var def in definitions)
            {
               Add(def, context, false);
            }

            context.SaveChanges();

            return true;
        }

        public bool DuplicateDefinitions()
        {
            using var context = _dbf.CreateDbContext();
            return context.DictionaryDefinitions
                          .GroupBy(p => new { p.Word, p.Definition })
                          .Where(p => p.Count() > 1)
                          .Any();
        }
    }
}

using Serilog;

namespace Infrastructure
{
    public class ExternalDictionaryManager : ManagerBase
    {
        public ExternalDictionaryManager(LinguineDbContextFactory db) : base(db)
        {
        }

        public List<String> AvailableDictionaries()
        {
           return _dbf.CreateDbContext().DictionaryDefinitions.Select(d => d.Source)
                                               .Distinct()
                                               .ToList();
        }

        public ExternalDictionary? GetFirstDictionary()
        {
            String name = AvailableDictionaries().FirstOrDefault();

            if (name is not null || name != "")
            {
                return GetDictionary(name);
            } else
            {
                return null;
            }
        }
        public ExternalDictionary? GetDictionary(String source)
        {
            if (!AvailableDictionaries().Contains(source))
            {
                Log.Warning("requested non-existent dictionary");
                return null;
            }

            return new ExternalDictionary(source, _dbf);
        }

        public void AddNewDictionaryFromCSV(String filename, String source)
        {
            if (AvailableDictionaries().Contains(source))
            {
                throw new InvalidDataException("naming conflict identified, dictionary adding aborted");
            }

            ExternalDictionary newDictionary = new ExternalDictionary(source, _dbf); // factory?

            ExternalDictionaryCSVParser.ParseDictionaryFromCSVToSQLiteAndSave(newDictionary, filename, source);

            VerifyIntegrity(newDictionary);
        }

        public void VerifyIntegrity(ExternalDictionary dictionary)
        {
            if (dictionary.DuplicateDefinitions() == true)
            {
                throw new Exception("found duplicate definitions");
            }
        }

    }
}

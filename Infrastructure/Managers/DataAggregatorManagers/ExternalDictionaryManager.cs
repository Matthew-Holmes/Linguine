using Infrastructure;
using Serilog;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Security;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

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

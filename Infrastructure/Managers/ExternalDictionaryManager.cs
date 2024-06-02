using Infrastructure;
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
        public ExternalDictionaryManager(LinguineDataHandler db) : base(db)
        {
        }

        public List<String> AvailableDictionaries()
        {
           return _db.DictionaryDefinitions.Select(d => d.Source)
                                           .Distinct()
                                           .ToList();
        }

        public ExternalDictionary? GetDictionary(String source)
        {
            if (!AvailableDictionaries().Contains(source))
            {
                return null;
            }

            return new ExternalDictionary(source, _db);
        }

        public void AddNewDictionaryFromCSV(String filename, String source)
        {
            if (AvailableDictionaries().Contains(source))
            {
                throw new InvalidDataException("naming conflict identified, dictionary adding aborted");
            }

            ExternalDictionary newDictionary = new ExternalDictionary(source, _db); // factory?

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

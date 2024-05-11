using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningStore
{
    public class ExternalDictionary : IDisposable
    {
        public LanguageCode LanguageCode { get; }
        public String Name { get; }

        private readonly ExternalDictionaryContext _context;

        public ExternalDictionary(LanguageCode lc, String name, String connectionString)
        {
            LanguageCode = lc;
            Name = name;

            _context = new ExternalDictionaryContext(connectionString);

            // Check if the database exists and can be connected to
            if (!_context.Database.CanConnect())
            {
                throw new InvalidOperationException("Cannot connect to the database.");
            }

            // Check if the database is empty
            if (!_context.DictionaryDefinitions.Any())
            {
                throw new InvalidOperationException("The database is empty.");
            }
        }

        public List<DictionaryDefinition> TryGetDefinition(String word)
        {
            return _context.DictionaryDefinitions.Where(dd => dd.Word == word).ToList();
        }

        public bool Contains(String word)
        {
            return _context.DictionaryDefinitions.Any(dd => dd.Word == word);
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }

}

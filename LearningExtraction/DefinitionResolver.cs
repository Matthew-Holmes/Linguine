using CsvHelper;
using LearningStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public class DefinitionResolver
    {
        private ExternalDictionary _dictionary;

        public DefinitionResolver(ExternalDictionary dictionary)
        {
            _dictionary = dictionary;
        }
        
        private List<List<DictionaryDefinition>> GetPossibleDefinitions(TextDecomposition td)
        {
            return td.Units.Select(u => _dictionary.TryGetDefinition(u.Text)).ToList();
        }

    }
}

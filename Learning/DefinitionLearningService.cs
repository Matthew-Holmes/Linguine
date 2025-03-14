using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning
{
    public class DefinitionLearningService
    {
        private ExternalDictionary                _dictionary;
        private ParsedDictionaryDefinitionManager _pdefManager;
        private StatementManager                  _statementManager;

        public DefinitionLearningService(
            ExternalDictionary                dictionary,
            ParsedDictionaryDefinitionManager pdefManager,
            StatementManager                  statementManager)
        {
            _dictionary       = dictionary;
            _pdefManager      = pdefManager;
            _statementManager = statementManager;
        }

        public DictionaryDefinition GetRandomDefinition()
        {
            return _dictionary.GetRandomDefinition();
        }
    }
}

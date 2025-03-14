using Infrastructure;
using Infrastructure.DataClasses;
using Serilog;
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

        public DictionaryDefinition GetFrequentDefinition()
        {
            if (DefinitionFrequencyEngine.DefinitionFrequencies is null)
            {
                Log.Error("need to compute the definition frequencies!");
                throw new Exception();
            }

            var eligibleKeys = DefinitionFrequencyEngine.DefinitionFrequencies
                .Where(kvp => kvp.Value > 2) // Filter for counts > 5
                .Select(kvp => kvp.Key)
                .ToList();

            if (eligibleKeys.Count == 0)
                return null; // No eligible keys found

            Random rnd = new Random();
            int def_key = eligibleKeys[rnd.Next(eligibleKeys.Count)];

            DictionaryDefinition? def = _dictionary.TryGetDefinitionByKey(def_key);

            if (def is null)
            {
                Log.Error("couldn't find a definition that was supposed to exist");
                throw new Exception(); // maybe update the frequencies then try again?
            }

            return def;
        }
    }
}

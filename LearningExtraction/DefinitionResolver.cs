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
        private TextDecomposition _injectiveDecomposition;
        private TextDecomposition _rootedDecomposition;

        public DefinitionResolver(ExternalDictionary dictionary, TextDecomposition injectiveDecomposition, TextDecomposition rootedDecomposition)
        {
            _dictionary = dictionary;

            if (injectiveDecomposition.Decomposition.Count != rootedDecomposition.Decomposition.Count)
            {
                throw new ArgumentException("rooted decomposition has different number of units to the injective decomposition");
            }

            if (injectiveDecomposition.Total.Text != rootedDecomposition.Total.Text)
            {
                throw new ArgumentException("decompositions' total texts differ");
            }

            _injectiveDecomposition = injectiveDecomposition;
            _rootedDecomposition = rootedDecomposition;

        }

    }
}

using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public  class TextDecompositionAndRootingAgent : OpenAIBase
    {

        private static String preamble = "Decompose the following text into units of meaning and their uninflected roots, either single words or phrases, do not split full names, nor split words when their composite is very common or cannot be naively inferred from their components, only respond using direct contents of the text, ignore punctuation unless it is essential to the meaning of the word, similarly don't add punctuation to the units unless essential, add no whitespace unless it already existed and conveyed meaning. Each OriginalUnit MUST directly and injectively map into the original text. Then append the root form of the unit, removing inflection i.e using infinitives, singular form etc, and using canonical lower vs uppercase, roots MUST not be uppercase if the unit can be lowercased. format \nOriginalUnit1\troot1\nOriginalUnit2\troot2 etc The set of OriginalUnit values must map injectively into the text"; 

        public TextDecompositionAndRootingAgent(string apiKey) : base(apiKey, preamble, 0, 4096 /* new turbo can do 16k */) { }

    }
}

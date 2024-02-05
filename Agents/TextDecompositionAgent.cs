using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class TextDecompositionAgent : OpenAIBase
    {
        private static String preamble = "Decompose the following text into units of meaning, usually words, unless they are multiword, such as full names or multiword nouns, if the meaning of multiple words is lost when they are considered in isolation don't split them, only respond using direct contents of the text, with each unit of meaning on a newline, ignore punctuation unless it is essential to the meaning of the word, similarly don't add punctuation to the units unless essential, the result must map injectively into the original text, add no whitespace unless it already existed and conveyed meaning, format \nunit1\nunit2 etc";

        public TextDecompositionAgent(string apiKey) : base(apiKey, preamble, 0, 1000) { }
    }
}

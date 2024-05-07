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
        private static String systemMessage = "Decompose the following text into units of meaning, either single words or phrases, do not split full names, nor split words when their composite is very common or cannot be naively inferred from their components - this is a coarse first pass so be generous with what is considered a composite word, only respond using direct contents of the text, with each unit of meaning on a newline, ignore punctuation unless it is essential to the meaning of the word, similarly don't add punctuation to the units unless essential, the result must map injectively into the original text, add no whitespace unless it already existed and conveyed meaning. Do not complete truncated words, all units should map into the original verbatim. format \nunit1\nunit2 etc";

        public TextDecompositionAgent(string apiKey, bool highPowered = false) : base(apiKey)
        {
            StringParameters["system"] = systemMessage;  

            if (highPowered)
            {
                StringParameters["model"] = "gpt-4-turbo";
            }
        }
    }
}
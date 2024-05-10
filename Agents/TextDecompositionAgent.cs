using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class TextDecompositionAgent : OpenAIProcessingBase
    {
        private static String systemMessage = "Decompose the following text into units of meaning, normally single words. Do not split compound words such as \nhot dog\n or \nfind out\n. Do not split names such as \nJoe Bloggs\n or \nUnited Kingdom\n. Only respond using direct contents of the text, with each unit of meaning on a newline, ignore punctuation and brackets that don't affect words' meaning, similarly don't add punctuation to the units unless essential, the result must map injectively into the original text, add no whitespace unless it already existed and conveyed meaning. Do not complete truncated words, all units should map into the original verbatim, do not skip words, you can skip punctuation. format \nunit1\nunit2 etc";

        public TextDecompositionAgent(string apiKey, bool highPowered = false) : base(apiKey)
        {
            StringParameters["system"] = systemMessage;
            //ContinousParameter("Temperature").Value = 1.0;
            //ContinousParameter("TopP").Value = 0.1;
            if (highPowered)
            {
                StringParameters["model"] = "gpt-4-turbo";
            }
        }
    }
}
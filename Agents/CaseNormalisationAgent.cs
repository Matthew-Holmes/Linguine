using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class CaseNormalisationAgent : OpenAIBase
    {
        //private static String systemMessage = "format the following words/phrases to be all lowercase, unless the word/phrase MUST include uppercase letters, such as proper nouns, place names, acronyms etc, in which case keep the required uppercase letters. format \nword1\nword2 etc";
        //private static String systemMessage = "remove any non-essential uppercase letters from the provided words/phrases, DO NOT remove uppercase from peoples' names, place names, etc, but revert to lowercase when possible, when the word/phrase can be written in that form. format \nword1\nword2 etc";
        private static String systemMessage = "format the following words/phrases to normalise uppercase vs lowercase, proper nouns, places, names should have initial letters uppercased, but words that have been uppercased due to punctuation should be reverted to their canonical form e.g. The\nman\ncalled\nKeith becomes the\nman\ncalled\nKeith. format \nword1\nword2 etc";
        public CaseNormalisationAgent(string apiKey) : base(apiKey)
        {
            StringParameters["system"] = systemMessage;
        }
    }
}

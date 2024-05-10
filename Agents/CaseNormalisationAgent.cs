using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class CaseNormalisationAgent : OpenAIProcessingBase
    {
        //private static String systemMessage = "format the following words/phrases to be all lowercase, unless the word/phrase MUST include uppercase letters, such as proper nouns, place names, acronyms etc, in which case keep the required uppercase letters. format \nword1\nword2 etc";
        //private static String systemMessage = "remove any non-essential uppercase letters from the provided words/phrases, DO NOT remove uppercase from peoples' names, place names, etc, but revert to lowercase when possible, when the word/phrase can be written in that form. format \nword1\nword2 etc";
        private static String systemMessage = "format the following words/phrases to the root form in terms of uppercase vs lowercase. uppercase initial letters in proper nouns, names, places etc. you must be able to explain why you chose to use uppercase, do not add the explanation. do not infer sentence structure, do not automatically uppercase the first word in a sentence unless it is always uppercase. Format \nword1\nword2 etc, you may recombine names and compound words across lines. Favour lowercase when unsure";

        public CaseNormalisationAgent(string apiKey) : base(apiKey)
        {
            //ContinousParameter("Temperature").Value = 0.01;
            //ContinousParameter("TopP").Value = 0.25;
            StringParameters["system"] = systemMessage;
        }
    }
}

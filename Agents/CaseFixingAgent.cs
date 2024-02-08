using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class CaseFixingAgent : OpenAIBase
    {
        private static String preamble = "Fill in the missing values, for each case provided, resetting any uppercase vs lowercase differences to the standard sentence case if the text is inside a sentence. Return the output using the exact same format as the input, make sure to copy the format of the input, copying the column titles every other line, but filling in missing values";
        //private static String preamble = "Below are two datasets, the first a direct breaking up of some text, the second ignores the original text and is each lines with standard uppercase vs lowercase rules of the language applied. The system has aided a learner by setting the lines to their default cases, words with the possibility to change the cases of characters have been set to the most commonly seen in isolation:";
        public CaseFixingAgent(string apiKey) : base(apiKey, preamble, 0, 1000, "gpt-3.5-turbo-0125", 0.1m) { }
    }
}

using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class UnitRootingAgent : OpenAIProcessingBase
    {
        private static String systemMessage = "process the following lines to remove inflection, converting verbs to the infinitive and nouns to the singular for example. Use standard English capitalisation, proper nouns, names, surnames, locations, place names, acronyms MUST obey capitisation rules, with the initial letter capitalised, otherwise favour lowercase. Remove unecessary punctutation and possesive apostrophes. You MUST NOT omit lines. DO NOT combine lines. DO NOT split lines. The number of output and input lines MUST be the same. Format \nword1\nword2 etc.";

        public UnitRootingAgent(string apiKey) : base(apiKey)
        {
            ContinousParameter("Temperature").Value = 1.0; // below 1 tends to recombine lines
            ContinousParameter("TopP").Value = 0.5;
            StringParameters["system"] = systemMessage;

            //StringParameters["model"] = "gpt-4o";
        }
    }
}

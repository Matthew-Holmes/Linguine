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
        private static String systemMessage = "return the roots of the following words/phrases. You should remove inflection, converting verbs to the infinitive and nouns to the singular for example. Remove unecessary punctutation. Make unecessary uppercase lowercase, keep proper nouns, names, places with initial uppercase. Format \nword1\nword2 etc. Recombine names and compound words that have been split across lines. Do not omit any lines";

        public UnitRootingAgent(string apiKey) : base(apiKey)
        {
            //ContinousParameter("Temperature").Value = 0.1;
            //ContinousParameter("TopP").Value = 0.25;
            StringParameters["system"] = systemMessage;
        }
    }
}

using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class DefinitionResolutionAgent : OpenAIProcessingBase
    {
        private static String systemMessage = "Given the following word, context and definition options, determine the most appropriate definition, respond with only a single integer value and nothing else using the indexing of the definitions given, if no definitions match, respond -1";
        public DefinitionResolutionAgent(string apiKey) : base(apiKey)
        {
            StringParameters["system"] = systemMessage;
            ContinousParameter("Temperature").Value = 0.01;
            //StringParameters["model"] = "gpt-4o";
        }
    }
}

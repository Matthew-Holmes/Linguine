using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class ContextUpdateAgent : OpenAIProcessingBase
    {
        private static String systemMessage = "Consider the following statements, and context, please update the context to reflect the changes that occur between the lines requested, preserve context elements that are still applicable, but remove and add lines of context so that the subsequent statements are comprehendible using that context. Respond with just the new context, with each item on a new line as before. DO NOT SUMMARISE - provide concise contextual information only.";

        public ContextUpdateAgent(string apiKey) : base(apiKey)
        {
            StringParameters["system"] = systemMessage;
        }
    }
}

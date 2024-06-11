using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class StatementGeneratorAgent : OpenAIProcessingBase
    {
        private static String systemMessage = "break the following text into the statements composing it, usually sentences for conventional text, they should be long enough that with context provided a user can understand the meaning of the statement; the statements must be verbatim from the text DO NOT SUMMARISE, DO NOT NUMBER THE STATEMENTS, the statements should map injectively into the text. Return each statement on a new line";

        public StatementGeneratorAgent(string apiKey, bool highPowered = false) : base(apiKey)
        {
            StringParameters["system"] = systemMessage;
            //ContinuousParameter("Temperature").Value = 1.0;
            //ContinuousParameter("TopP").Value = 0.1;
            if (highPowered)
            {
                StringParameters["model"] = "gpt-4o";
            }
        }
    }
}

using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class ContextChangeIdentificationAgent : OpenAIProcessingBase
    {
        private static String systemMessage = "Given the following numbered statements and context, identify when the context changes, if it does. Return the each number on a new line, with a brief description of why the context is changing, return format (x,y, etc line numbers): x: reason\ny: reason\n...";
        public ContextChangeIdentificationAgent(string apiKey) : base(apiKey)
        {
            StringParameters["system"] = systemMessage;
        }
    }
}

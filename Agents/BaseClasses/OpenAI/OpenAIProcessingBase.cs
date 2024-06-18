using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents.OpenAI
{
    public class OpenAIProcessingBase : OpenAIBase
    {
        // Hide sequential history
        new private List<Tuple<String, String>> SequentialPromptLog = new List<Tuple<string, string>>();

        public OpenAIProcessingBase(string apiKey) : base(apiKey)
        {
            DiscreteParameter("PromptDepth").Value = 0;
            MaxConcurrentResponses = 5;
        }
    }
}

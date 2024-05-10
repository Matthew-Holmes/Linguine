using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents.OpenAI
{
    public class OpenAIConversingBase : OpenAIBase
    {
        // Hide the concurrent features
        new private int MaxConcurrentResponses = 0; 
        new private ConcurrentBag<Tuple<String, String>> ConcurrentPromptLog = new ConcurrentBag<Tuple<string, string>>();

        public OpenAIConversingBase(String apiKey) : base(apiKey) 
        {
            base.MaxConcurrentResponses = 1;
        }
    }
}

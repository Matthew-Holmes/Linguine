using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class RootValidatorAgent : OpenAIBase
    {
        private static String preamble = "Given the context, is the provided root of original correct. ONLY reply y or n";
        public RootValidatorAgent(string apiKey) : base(apiKey, preamble, 0, 1000) { }

    }
}

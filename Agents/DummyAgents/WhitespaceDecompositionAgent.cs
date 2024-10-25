using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Helpers;
using Infrastructure;

namespace Agents.DummyAgents
{
    public class WhitespaceDecompositionAgent : AgentBase
    {
        protected override Task<String> GetResponseCore(String prompt)
        {
            String response = "";

            foreach(String s in prompt.Split(null))
            {
                response += StringHelper.StripOuterPunctuation(s);
                response += '\n';
            }

            response = response.Substring(0, response.Length - 1);

            return Task.FromResult(response);
        }

        public WhitespaceDecompositionAgent()
        {
            //AgentTask = AgentTask.DecompositionToUnits;
            //LLM       = LLM.Dummy;
        }
    }
}

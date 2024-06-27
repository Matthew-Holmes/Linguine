using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents.DummyAgents
{
    public class SentenceDecompositionAgent : AgentBase
    {
        protected override Task<String> GetResponseCore(String prompt)
        {
            String response = "";

            foreach (String s in prompt.Split('.'))
            {
                response += s;
                response += '\n';
            }

            response = response.Substring(0, response.Length - 1);

            return Task.FromResult(response);
        }

        public SentenceDecompositionAgent()
        {
            AgentTask = AgentTask.DecompositionToStatements;
            LLM       = LLM.Dummy;
        }
    }
}

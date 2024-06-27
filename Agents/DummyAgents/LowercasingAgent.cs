using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents.DummyAgents
{
    public class LowercasingAgent : AgentBase
    {
        protected override Task<string> GetResponseCore(string prompt)
        {
            return Task.FromResult(prompt.ToLower());
        }

        public LowercasingAgent()
        {
            AgentTask = AgentTask.UnitRooting;
            LLM = LLM.Dummy;
        }

    }
}

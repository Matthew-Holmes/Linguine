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
            //AgentTask = AgentTask.UnitRooting;
            //LLM       = LLM.Dummy;
        }

    }
}

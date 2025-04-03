namespace Agents.DummyAgents
{
    public class DummyTextDecompositionAgent : AgentBase
    {
        protected override Task<String> GetResponseCore(String prompt)
        {

            int len = prompt.Length;

            String response = len > 5 ? prompt.Substring(0, 5) : prompt;

            int lhs = 5;
            int rhs = 10;

            while (rhs <= len)
            {
                response += '\n';
                response += len > lhs + 5 ? prompt.Substring(lhs, 5) : prompt.Substring(lhs);
                lhs = rhs;
                rhs += 5;
            }

            return Task.FromResult(response);
        }

        public DummyTextDecompositionAgent()
        {
            //AgentTask = AgentTask.DecompositionToUnits;
            //LLM       = LLM.Dummy;
        }
    }
}

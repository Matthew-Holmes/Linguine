namespace Agents.DummyAgents
{
    public class SentenceDecompositionAgent : AgentBase
    {
        protected override Task<String> GetResponseCore(String prompt)
        {
            String response = "";
            String[] delimiters = new[] { "。", ".", ",", ";", " " };
            String[] parts = new[] { prompt };

            foreach (String delimiter in delimiters)
            {
                var split = prompt.Split(new[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
                if (split.Length > 1)
                {
                    parts = split;
                    break;
                }
            }

            foreach (String s in parts)
            {
                response += s + '\n';
            }

            if (response.Length > 0)
                response = response.Substring(0, response.Length - 1); // remove trailing newline

            return Task.FromResult(response);
        }


        public SentenceDecompositionAgent()
        {
            //AgentTask = AgentTask.DecompositionToStatements;
            //LLM       = LLM.Dummy;
        }
    }
}

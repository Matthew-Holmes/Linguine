using Agents;
using System.Diagnostics;
using DataClasses;

namespace LearningExtraction
{
    public static class DecompositionTransformerBijective
    {
        public static async Task<TextDecomposition> ApplyAgent(AgentBase agent, TextDecomposition source, int retry = 2, bool trim=false)
        {
            // prompts the agent with a prompt derived from each decomposition unit on each line
            // the response is converted to a response decomposition

            // ensures that as many lines of response as in prompt
            if ((source.Decomposition?.Count ?? 0) == 0)
            {
                return TextDecomposition.FromNewLinedString(source.Total, "");
            }

            String prompt = String.Join('\n', source.Decomposition.Select(unit => unit.Total));

            String response = await GetResponse(agent, prompt, retry); // agent best at identifying lower --> upper, not the other way around

            return TextDecomposition.FromNewLinedString(source.Total, response, trim);
        }

        private static bool Bijects(String prompt, String response)
        {
            return prompt.Split('\n').Count() == response.Split('\n').Count();
        }

        private static async Task<String> GetResponse(AgentBase agent, string prompt, int retry)
        {
            String newLinedResponse = await agent.GetResponse(prompt);

            for (int j = 0; j != retry && !Bijects(prompt, newLinedResponse); j++)
            {
                newLinedResponse = await agent.GetResponse(prompt);
            }

            if (!Bijects(prompt, newLinedResponse))
            {
                // bijectivity compromised
                // revert to identity map
                newLinedResponse = prompt;
                Debug.WriteLine("unit rooting reverted to identity map");
            }

            return newLinedResponse;
        }
    }
}

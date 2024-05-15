using Agents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public static class DecompositionTransformerBijective
    {
        public static async Task<TextDecomposition> ApplyAgent(AgentBase agent, TextDecomposition source, int maxCharsToProcess, int joinLines, int retry = 2)
        {
            // prompts the agent with a prompt derived from each decomposition unit on each line
            // the response is converted to a response decomposition

            // applies windowing if the prompt would be too big

            // ensures that as many lines of response as in prompt

            String prompt = String.Join('\n', source.Decomposition.Select(unit => unit.Total.Text));

            String response = await GetCombinedResponses(agent, prompt, maxCharsToProcess, joinLines, retry); // agent best at identifying lower --> upper, not the other way around

            return TextDecomposition.FromNewLinedString(source.Total.Text, response);
        }

        private static async Task<String> GetCombinedResponses(AgentBase agent, string prompt, int maxCharsToProcess, int joinLines, int retry)
        {
            // parallel windows strategy
            if (prompt.Length > maxCharsToProcess)
            {
                (List<String> prompts, int joinLinesUsed) = DecompositionHelper.Window(prompt, maxCharsToProcess, joinLines);

                var getResponseTasks = prompts.Select(prompt => agent.GetResponse(prompt));

                String[] responses = await Task.WhenAll(getResponseTasks);

                for (int i = 0; i != prompts.Count; i++)
                {
                    for( int j = 0; j != retry || prompts[i].Split('\n').Count() != responses[i].Split('\n').Count(); j++)
                    {
                        // bijectivity compromised, try again
                        responses[i] = await agent.GetResponse(prompts[i]);
                    }

                    if (prompts[i].Split('\n').Count() != responses[i].Split('\n').Count())
                    {
                        // bijectivity compromised
                        // revert to identity map
                        responses[i] = prompts[i];
                    }
                }

                return Combine(responses.ToList(), joinLinesUsed);
            }

            String newLinedResponse = await agent.GetResponse(prompt);

            for (int j = 0; j != retry && prompt.Split('\n').Count() != newLinedResponse.Split('\n').Count(); j++)
            {
                // bijectivity compromised, try again
                newLinedResponse = await agent.GetResponse(prompt);
            }

            if (prompt.Split('\n').Count() != newLinedResponse.Split('\n').Count())
            {
                // bijectivity compromised
                // revert to identity map
                newLinedResponse = prompt;
            }

            return newLinedResponse;
        }

        private static string Combine(List<string> list, int joinLines)
        {
            // undo the windowing

            if (list.Count == 1)
            {
                return list[0];
            }

            List<String> lhs = list[0].Split('\n').ToList();
            List<String> rhs = list[1].Split('\n').ToList();

            int overlap = joinLines;

            int rhsDrop = overlap / 2;
            int lhsDrop = overlap - rhsDrop;

            rhs = rhs.Skip(rhsDrop).ToList();
            lhs.RemoveRange(lhs.Count - lhsDrop - 1, lhsDrop);

            lhs.AddRange(rhs);

            List<String> reduced = new List<String> { String.Join('\n', lhs) };
            reduced.AddRange(list.Skip(2));

            return Combine(reduced, joinLines);
        }
    }
}

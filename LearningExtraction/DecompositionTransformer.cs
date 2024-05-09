using Agents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public static class DecompositionTransformer
    {
        public static async Task<TextDecomposition> ApplyAgent(AgentBase agent, TextDecomposition source, int maxCharsToProcess, int joinChars)
        {
            // prompts the agent with a prompt derived from each decomposition unit on each line
            // the response is converted to a response decomposition

            // applies windowing if the prompt would be too big

            String prompt = String.Join('\n', source.Units.Select(unit => unit.Total.Text)).ToLower();

            String response = await GetCombinedResponses(agent, prompt, maxCharsToProcess, joinChars); // agent best at identifying lower --> upper, not the other way around

            return TextDecomposition.FromNewLinedString(source.Total.Text, response);
        }

        private static async Task<String> GetCombinedResponses(AgentBase agent, string prompt, int maxCharsToProcess, int joinChars)
        {
            if (prompt.Length > maxCharsToProcess)
            {
                List<String> prompts = Window(prompt, maxCharsToProcess, joinChars);

                var getResponseTasks = prompts.Select(prompt => agent.GetResponse(prompt));

                String[] responses = await Task.WhenAll(getResponseTasks);

                return Combine(responses.ToList());
            }

            String newLinedResponse = await agent.GetResponse(prompt); // only called in the base case

            return newLinedResponse;
        }

        private static string Combine(List<string> list)
        {
            if (list.Count == 1)
            {
                return list[0];
            }

            List<String> lhs = list[0].Split('\n').ToList();
            List<String> rhs = list[1].Split('\n').ToList();

            rhs = rhs.Skip(DetermineOverlap(lhs, rhs)).ToList();

            lhs.AddRange(rhs);

            List<String> reduced = new List<String> { String.Join('\n', lhs) };
            reduced.AddRange(list.Skip(2));

            return Combine(reduced);
        }

        private static int DetermineOverlap(List<String>? lhs, List<String>? rhs)
        {
            int ret = 0;

            if ((lhs?.Count ?? 0) == 0 || (rhs?.Count ?? 0) == 0)
            {
                return ret;
            }

            for (int i = 0; i < Math.Min(lhs.Count, rhs.Count); i++)
            {
                bool overlap = true;
                for (int j = 0; j != i; j++)
                {
                    overlap = overlap && lhs[lhs.Count - i + j] == rhs[j];
                }
                if (overlap)
                {
                    ret = i;
                } // when the loop is done we'll get the biggest that worked

                // N^2 runtime --> watch out for slowdown if this handles big stuff
            }

            return ret;

        }

        private static List<String> Window(String original, int maxCharsToProcess, int joinChars)
        {
            // base case
            if (original.Count() <= maxCharsToProcess)
            {
                return new List<String> { original };
            }

            List<String> ret = new List<string>();

            int rightSlice = maxCharsToProcess;
            int leftSlice = maxCharsToProcess;

            while (original[rightSlice] != '\n' && rightSlice != 0)
            {
                rightSlice--;
            }

            // do the first window
            while (leftSlice > maxCharsToProcess - joinChars && original[leftSlice] != '\n')
            {
                leftSlice--;
            }

            ret.Add(original.Substring(0, rightSlice));

            // recurse
            ret.AddRange(Window(original.Substring(leftSlice), maxCharsToProcess, joinChars));

            return ret;
        }
    }
}

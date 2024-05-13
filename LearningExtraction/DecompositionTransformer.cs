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
        public static async Task<TextDecomposition> ApplyAgent(AgentBase agent, TextDecomposition source, int maxCharsToProcess, int joinLines)
        {
            // prompts the agent with a prompt derived from each decomposition unit on each line
            // the response is converted to a response decomposition

            // applies windowing if the prompt would be too big

            String prompt = String.Join('\n', source.Units.Select(unit => unit.Total.Text));

            String response = await GetCombinedResponses(agent, prompt, maxCharsToProcess, joinLines); // agent best at identifying lower --> upper, not the other way around

            return TextDecomposition.FromNewLinedString(source.Total.Text, response);
        }

        private static async Task<String> GetCombinedResponses(AgentBase agent, string prompt, int maxCharsToProcess, int joinLines)
        {
            if (prompt.Length > maxCharsToProcess)
            {

                (List<String> prompts, int _) = Window(prompt, maxCharsToProcess, joinLines);

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

            // warning - will fail if there are lots of repeated words
            int overlap = DetermineOverlap(lhs, rhs);

            int rhsDrop = overlap / 2;
            int lhsDrop = overlap - rhsDrop;

            rhs = rhs.Skip(rhsDrop).ToList();
            lhs.RemoveRange(lhs.Count - lhsDrop - 1, lhsDrop);

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

            for (int i = 0; i <= Math.Min(lhs.Count, rhs.Count); i++)
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

        internal static Tuple<List<String>, int> Window(String original, int maxCharsToProcess, int joinLines)
        {
            // handles too large join sizes
            // has to return the join size in case it was reduced

            try
            {
                return Tuple.Create(WindowInternal(original, maxCharsToProcess, joinLines), joinLines);
                    
            } catch (JoinException _)
            {
                return Window(original, maxCharsToProcess, joinLines / 2); // in edge case where the join lines were too many
            }
        }

        private static List<String> WindowInternal(String original, int maxCharsToProcess, int joinLines)
        {
            // base case
            if (original.Count() <= maxCharsToProcess)
            {
                return new List<String> { original };
            }

            List<String> ret = new List<string>();

            int rightSlice = maxCharsToProcess;
            int leftSlice;

            // make the first window
            
            while (original[rightSlice] != '\n' && rightSlice > 0)
            {
                rightSlice--;
            }

            rightSlice--; // consume the newline
            leftSlice = rightSlice;

            // form the join
            for (int i = 0; i != joinLines; i++)
            {
                while (leftSlice >0 && original[leftSlice] != '\n')
                {
                    leftSlice--;
                }
                leftSlice--; // consume the newline
            }

            if (leftSlice <= 0)
            {
                // join too big
                throw new JoinException();
            }

            ret.Add(original.Substring(0, rightSlice+1));

            // recurse
            ret.AddRange(WindowInternal(original.Substring(leftSlice+2), maxCharsToProcess, joinLines));

            return ret;
        }

        private class JoinException : Exception
        { };

    }
}

using Agents;
using ExternalMedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public class CaseNormaliser
    {
        public int MaxVolumeToProcess { get; set; } = 100; // if given text larger than this, chunk it
        public int JoinCharacterCount { get; set; } = 20;

        public AgentBase Agent { get; set; }

        public async Task<TextDecomposition> NormaliseCases(TextDecomposition priorDecomposition)
        {
            if (priorDecomposition.Units == null)
            {
                throw new ArgumentException("No units to normalise");
            }

            String prompt = String.Join('\n', priorDecomposition.Units.Select(unit => unit.Total.Text));

            String response = await NormaliseCases(prompt);

            return TextDecomposition.FromNewLinedString(priorDecomposition.Total.Text, response);

        }
           

        public async Task<String> NormaliseCases(String newLinedString)
        {
            if (newLinedString.Length > MaxVolumeToProcess)
            {
                List<String> prompts = Window(newLinedString);

                var normaliseTasks = prompts.Select(prompt => NormaliseCases(prompt));

                String[] responses = await Task.WhenAll(normaliseTasks);

                return Combine(responses.ToList());
            }

            String newLinedResponse = await Agent.GetResponse(newLinedString); // only called in the base case

            return newLinedResponse;
        }

        private string Combine(List<String> list)
        {
            if (list.Count == 1)
            {
                return list[0]; 
            }

            List<String> lhs  = list[0].Split('\n').ToList();
            List<String> rhs = list[1].Split('\n').ToList();

            rhs = rhs.Skip(DetermineOverlap(lhs, rhs)).ToList();

            lhs.AddRange(rhs);

            List<String> reduced = new List<String> { String.Join('\n', lhs) };
            reduced.AddRange(list.Skip(2));

            return Combine(reduced);
        }

        // TODO - duplicated code, share a method with TextDecomposer
        private int DetermineOverlap(List<String>? lhs, List<String>? rhs)
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

        private List<String> Window(String original)
        {
            // base case
            if (original.Count() <= MaxVolumeToProcess)
            {
                return new List<String> { original };
            }

            List<String> ret = new List<string>();

            int rightSlice = MaxVolumeToProcess;
            int leftSlice = MaxVolumeToProcess;

            while (original[rightSlice] != '\n' && rightSlice != 0)
            {
                rightSlice--;
            }

            // do the first window
            while (leftSlice > MaxVolumeToProcess - JoinCharacterCount && original[leftSlice] != '\n')
            {
                leftSlice--;
            }

            ret.Add(original.Substring(0, rightSlice));

            // recurse
            ret.AddRange(Window(original.Substring(leftSlice)));

            return ret;
        }

    }
}

using Agents;
using ExternalMedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public class TextDecomposerAndRooter
    {
        public int MaxVolumeToProcess { get; set; } // if given text larger than this, chunk it

        public AgentBase Agent { get; set; } // should decomposition as word1\troot1\nword2\troot2\n...

        public async Task<(TextDecomposition,TextDecomposition)> DecomposeText(TextualMedia textSource, bool mustInject = true, bool mustBiject = false)
        {
            if (textSource.Text.Length > MaxVolumeToProcess)
            {
                throw new NotImplementedException("need to implement text chunking");
            }

            String response = await Agent.GetResponse(textSource.Text);

            // pick apart the response
            String wordsPattern = @"([^\t\n]+)\t";
            // Regex to match all definitions
            String rootsPattern = @"\t([^\n]+)";

            // Extracting words
            var wordsMatches = Regex.Matches(response, wordsPattern);
            String words = string.Join("\n", from Match m in wordsMatches select m.Groups[1].Value);

            // Extracting definitions
            var rootsMatches = Regex.Matches(response, rootsPattern);
            String roots = string.Join("\n", from Match m in rootsMatches select m.Groups[1].Value);

            TextDecomposition unitsDecomposition = TextDecomposition.FromNewLinedString(textSource.Text, words);
            TextDecomposition rootsDecomposition = TextDecomposition.FromNewLinedString(textSource.Text, roots);

            if (mustBiject && !unitsDecomposition.Bijects())
            {
                throw new Exception("Invalid decomposition");
            }

            if (mustInject && !unitsDecomposition.Injects())
            {
                throw new Exception("Invalid decomposition");
            }

            return (unitsDecomposition, rootsDecomposition);
        }

        public TextDecomposerAndRooter(int maxVolumeToProcess, AgentBase agent)
        {
            MaxVolumeToProcess = maxVolumeToProcess;
            Agent = agent;
        }
    }
}

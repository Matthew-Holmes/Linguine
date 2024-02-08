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

        public AgentBase Agent { get; set; } // should return decomposition as word1\troot1\nword2\troot2\n...
        public AgentBase CaseFixerAgent { get; set; } // fixes cases on roots, should return root1\nroot2\n etc

        public AgentBase CasePossibilitiesAgent { get; set; }
        public AgentBase CaseChoosingAgent { get; set; }

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
            //String roots = String.Join("\n", from Match m in rootsMatches select m.Groups[1].Value);

            var tableBuilder = new System.Text.StringBuilder();

            // Header to be repeated
            var headers = "Original\tCase Normalised\n";

            // Iterate through each match and add it with headers
            foreach (Match m in rootsMatches)
            {
                // Add table headers before each row
                tableBuilder.Append(headers);

                // Extract the value for the "Original" column
                var originalValue = m.Groups[1].Value;

                // Leave the "Standard form" column empty for now
                var standardFormValue = "";

                // Append the row to the table
                tableBuilder.AppendLine($"{originalValue}\t{standardFormValue}");
            }

            // Convert the StringBuilder to a string
            var tableString = tableBuilder.ToString();


            String filledInTable = await CaseFixerAgent.GetResponse(tableString);
            var lines = filledInTable.Trim().Split('\n');

            // Initialize a variable for the extracted values
            var fixedRoots = "";

            // Iterate through each line, skipping the header lines
            for (int i = 1; i < lines.Length; i += 2) // Start from the second line and skip every other line (headers)
            {
                // Split each line by tab to get columns
                var columns = lines[i].Split('\t');

                // Check if the line has at least two columns and the second column is not empty
                if (columns.Length > 1 && !string.IsNullOrWhiteSpace(columns[1]))
                {
                    // Add the value from the "Standard form" column to the output string
                    fixedRoots += columns[1] + "\n"; // Use Environment.NewLine if you prefer
                }
            }

            // Trim the last newline character
            fixedRoots = fixedRoots.TrimEnd('\n');



            TextDecomposition unitsDecomposition = TextDecomposition.FromNewLinedString(textSource.Text, words);
            TextDecomposition rootsDecomposition = TextDecomposition.FromNewLinedString(textSource.Text, fixedRoots);

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

        public TextDecomposerAndRooter(int maxVolumeToProcess, AgentBase agent, AgentBase caseFixer, AgentBase casePossibilities, AgentBase caseChooser)
        {
            MaxVolumeToProcess = maxVolumeToProcess;
            Agent = agent;
            CaseFixerAgent = caseFixer;
            CasePossibilitiesAgent = casePossibilities;
            CaseChoosingAgent = caseChooser;
        }

        private static IEnumerable<List<T>> SplitList<T>(T[] list, int chunkSize)
        {
            for (int i = 0; i < list.Length; i += chunkSize)
            {
                yield return new List<T>(list.Skip(i).Take(chunkSize));
            }
        }
    }
}

using Agents;
using LearningStore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public class RootResolver
    {
        public int LHSContextCharacters { get; set; }
        public int RHSContextCharacters { get; set; }
        public int MaxWordChars => 250; // something has definitely gone wrong if trying to analyse "words" this big

        private Variants _variants;

        public AgentBase ResolutionOptionsChooser { get; set; }
        public AgentBase ResolutionValidator { get; set; }
        public AgentBase ResolutionGenerator { get; set; }

        public RootResolver(int lHSContextCharacters, int rHSContextCharacters, Variants lookups)
        {
            LHSContextCharacters = lHSContextCharacters;
            RHSContextCharacters = rHSContextCharacters;

            _variants = lookups;
        }

        public async Task<TextDecomposition> ResolveRoots(TextDecomposition text)
        {
            if (!text.Injects())
            {
                // will struggle to find meaningful contexts if this is not the case
                throw new Exception("Invalid usage, expects injectivity");
            }
            
            if ((text.Units?.Count ?? 0) != (text.Flattened().Units?.Count ?? 0))
            {
                throw new NotImplementedException("Root resolution not yet implemented for heirarchical decompositions");
            }

            var ret = new TextDecomposition(new TextUnit(text.Total.Text), new List<TextDecomposition>()); // TODO - helper "empty copy constructor"?

            int pointer = 0;

            foreach(TextUnit unit in text.Units?.Select(td => td.Total) ?? new List<TextUnit>())
            {
                // find the context
                pointer += text.Total.Text.Substring(pointer).IndexOf(unit.Text); // assured found by injectivity

                int lhsPointer = Math.Max(0,                      pointer - LHSContextCharacters);
                int rhsPointer = Math.Min(text.Total.Text.Length, pointer + unit.Text.Length + RHSContextCharacters);

                String context = text.Total.Text.Substring(lhsPointer, rhsPointer - lhsPointer);

                // use agents to find a root
                List<String> possibilities = _variants.GetRoots(unit.Text).ToList();
                String root=""; bool foundValidRoot = false;

                // choose from possible variants
                if (possibilities.Count > 1)
                {
                    (root,foundValidRoot) = await ResolveFromOptions(unit.Text, possibilities, context);
                }
                
                // or validate if just one option
                if (possibilities.Count == 1)
                {
                    if (await ValidationResolution(unit.Text, possibilities[0], context))
                    {
                        root = possibilities[0];
                        foundValidRoot = true;
                    }
                } 

                // final option is to generate 
                if (!foundValidRoot)
                {
                    root = await GenerateRoot(unit.Text, context);
                }

                ret.Units?.Add(new TextDecomposition(new TextUnit(root), null) /* add as leaf */);
            }

            return ret;

        }

        private async Task<String> GenerateRoot(String original, String context)
        {
            StringBuilder promptBuilder = new StringBuilder();
            promptBuilder.AppendLine($"Original: {original}");
            promptBuilder.AppendLine($"Context: \"{context}\"");

            return await ResolutionGenerator.GetResponse(promptBuilder.ToString());
            // could add a validation step here?
        }

        private async Task<bool> ValidationResolution(String original, String potentialRoot, String context)
        {
            StringBuilder promptBuilder = new StringBuilder();
            promptBuilder.AppendLine($"Original: {original}");
            promptBuilder.AppendLine($"Potential Root: {potentialRoot}");
            promptBuilder.AppendLine($"Context: \"{context}\"");

            String response = await ResolutionValidator.GetResponse(promptBuilder.ToString());

            if (response == "y" || response == "Y")
            {
                return true;
            }

            if (response == "n" || response == "N")
            {
                return false;
            }

            throw new Exception("API validation failed");

        }

        private async Task<(String potentialRoot, bool isRight)> ResolveFromOptions(String original, List<String> possibilities,String context)
        {
            StringBuilder promptBuilder = new StringBuilder();
            promptBuilder.AppendLine($"Original: {original}");
            promptBuilder.AppendLine($"Context: \"{context}\"");
            promptBuilder.AppendLine("Options:");
            for(int i = 0; i != possibilities.Count; i++)
            {
                promptBuilder.AppendLine($"{i + 1}: {possibilities[i]}");
            }

            String chosen = await ResolutionOptionsChooser.GetResponse(promptBuilder.ToString());

            int indexOneIndexed;
            if( !int.TryParse(Regex.Replace(chosen, "[^0-9]", ""), out indexOneIndexed))
            {
                throw new Exception("API index selection failed");
            }

            if (indexOneIndexed < -1 || indexOneIndexed > possibilities.Count)
            {
                throw new Exception("API index selection failed");
            }

            if (indexOneIndexed == 0 || indexOneIndexed == -1)
            {
                return ("", false); // API decided that none of the roots where a match
            }
            else
            {
                return (possibilities[indexOneIndexed - 1], true);
            }
        }
    }
}

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

            // we do lowercase --> uppercase
            TextDecomposition allLower = DecompositionTransformer.ApplyAgent(
                new Agents.DummyAgents.LowercasingAgent(), priorDecomposition, MaxVolumeToProcess, JoinCharacterCount).Result;

            return await DecompositionTransformer.ApplyAgent(Agent, allLower, MaxVolumeToProcess, JoinCharacterCount);
        }
    }
}

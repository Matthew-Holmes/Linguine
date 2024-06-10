using Agents;
using ExternalMedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;

namespace LearningExtraction
{
    public class CaseNormaliser
    {
        public int MaxVolumeToProcess { get; set; } = 100; // if given text larger than this, chunk it
        public int JoinLineCount { get; set; } = 3;

        public AgentBase Agent { get; set; }

        public async Task<TextDecomposition> NormaliseCases(TextDecomposition priorDecomposition)
        {
            if (priorDecomposition.Decomposition == null)
            {
                throw new ArgumentException("No units to normalise");
            }

            // we do lowercase --> uppercase
            TextDecomposition allLower = DecompositionTransformerBijective.ApplyAgent(
                new Agents.DummyAgents.LowercasingAgent(), priorDecomposition, MaxVolumeToProcess, JoinLineCount).Result;

            return await DecompositionTransformerBijective.ApplyAgent(Agent, allLower, MaxVolumeToProcess, JoinLineCount);
        }
    }
}

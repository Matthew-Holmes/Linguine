using Agents;
using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public class UnitRooter
    {
        public int MaxVolumeToProcess { get; set; } = 200; // if given text larger than this, chunk it
        public int JoinCharacterCount { get; set; } = 20;

        public AgentBase Agent { get; set; }

        public async Task<TextDecomposition> RootUnits(TextDecomposition priorDecomposition)
        {
            if (priorDecomposition.Units == null)
            {
                throw new ArgumentException("No units to normalise");
            }

            return await DecompositionTransformer.ApplyAgent(Agent, priorDecomposition, MaxVolumeToProcess, JoinCharacterCount);
        }
    }
}

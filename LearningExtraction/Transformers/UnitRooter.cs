using Agents;
using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;

namespace LearningExtraction
{
    public class UnitRooter
    {
        public int MaxVolumeToProcess { get; set; } = 250; // if given text larger than this, chunk it
        public int JoinLineCount { get; set; } = 3;

        public AgentBase Agent { get; set; }

        public async Task<TextDecomposition> RootUnits(TextDecomposition priorDecomposition)
        {

            TextDecomposition ret = await DecompositionTransformerBijective.ApplyAgent(Agent, priorDecomposition, MaxVolumeToProcess, JoinLineCount, 0);

            return ret;
        }
    }
}

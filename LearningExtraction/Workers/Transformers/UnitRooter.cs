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

        public AgentBase Agent { get; set; }

        public async Task<TextDecomposition> RootUnits(TextDecomposition priorDecomposition)
        {

            TextDecomposition ret = await DecompositionTransformerBijective.ApplyAgent(
                Agent, priorDecomposition, MaxVolumeToProcess, 0, true);
            // TODO - domain (Chinese) specific stuff being encoded here - should add some 
            // sort of policy infrastructure to store this information in a codified way
            return ret;
        }
    }
}

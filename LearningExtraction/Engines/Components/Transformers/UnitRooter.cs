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
        // TODO - should I add a parallel windows strategy for long statements, if the agent
        // struggles with the rooting?
        public AgentBase Agent { get; set; }

        public async Task<TextDecomposition> RootUnits(TextDecomposition priorDecomposition)
        {

            TextDecomposition ret = await DecompositionTransformerBijective.ApplyAgent(
                Agent, priorDecomposition, 0, true);
            // TODO - domain (Chinese) specific stuff being encoded here - should add some 
            // sort of policy infrastructure to store this information in a codified way
            return ret;
        }
    }
}

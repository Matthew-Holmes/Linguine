using ExternalMedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agents;
using System.Diagnostics;

namespace LearningExtraction
{
    public class TextDecomposer
    {
        // TODO - write some tests with a mocked up agent that just does dummy split

        public int MaxVolumeToProcess { get; set; } // if given text larger than this, chunk it

        public AgentBase Agent { get; set; }

        public async Task<TextDecomposition> DecomposeText(TextualMedia textSource, bool mustInject = true, bool mustBiject = false)
        {
            if (textSource.Text.Length > MaxVolumeToProcess)
            {
                throw new NotImplementedException("need to implement text chunking");
            }

            String newLinedDecomposition = await Agent.GetResponse(textSource.Text);

            TextDecomposition ret = TextDecomposition.FromNewLinedString(textSource.Text, newLinedDecomposition);

            if (mustBiject && !ret.Bijects())
            {
                throw new Exception("Invalid decomposition");
            }

            if (mustInject && !ret.Injects())
            {
                throw new Exception("Invalid decomposition");
            }

            return ret;
        }

        public TextDecomposer(int maxVolumeToProcess, AgentBase agent)
        {
            MaxVolumeToProcess = maxVolumeToProcess;
            Agent = agent;
        }
    }
}

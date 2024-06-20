using ExternalMedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agents;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Infrastructure;

namespace LearningExtraction
{
    public class TextDecomposer
    {
        public int MaxVolumeToProcess { get; set; }  // if given text larger than this, chunk it

        public AgentBase StandardAgent { get; set; }
        public AgentBase HighPerformanceAgent { get; set; }
        public AgentBase FallbackAgent { get; set; }

        public async Task<TextDecomposition> DecomposeText(String text, bool mustInject = true, bool mustBiject = false)
        {
            if (text.Length > MaxVolumeToProcess)
            {
                throw new ArgumentException("Text too long to process");
            }

            TextDecomposition ret;
            Func<TextDecomposition, bool> MaintainsInvariants = (TextDecomposition td)
                => (!mustBiject || td.Bijects()) && (!mustInject || td.Injects());

            // first pass at the decomposition, using StandardAgent
            ret = await FromTextUsingAgent(text, StandardAgent);
            if (MaintainsInvariants(ret))
            {
                return ret;
            }

            // first attempt failed, use more powerful agent (e.g. higher spec LLM)
            ret = await FromTextUsingAgent(text, HighPerformanceAgent);

            if (MaintainsInvariants(ret))
            {
                return ret;
            }

            // do default, recommendation: use dummy agent guaranteed to maintain invariants
            ret = await FromTextUsingAgent(text, FallbackAgent);
            if (MaintainsInvariants(ret))
            {
                return ret;
            }
            else
            {
                throw new Exception("Invalid decomposition");
            }
        }

        private async Task<TextDecomposition> FromTextUsingAgent(String text, AgentBase agent)
        {
            return TextDecomposition.FromNewLinedString(text, await agent.GetResponse(text));
        }

        public TextDecomposer()
        {
        }
    }
}

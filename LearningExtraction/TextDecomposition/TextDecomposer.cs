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
using System.Text.RegularExpressions;
using Helpers;

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

            Func<TextDecomposition, bool> IsNonTrivial = (TextDecomposition td)
                => (td.Decomposition?.Count ?? 1) > 1;

            // first pass at the decomposition, using StandardAgent

            for (int i = 1; i != 5; i++)
            {
                // 4o-min is ~16x cheaper than 4o, thus we should try multiple times before resorting to
                // the high powered agent
                ret = await FromTextUsingAgent(text, StandardAgent);
                if (MaintainsInvariants(ret) & IsNonTrivial(ret))
                {
                    Debug.WriteLine($"successful decomposition after {i} attempts");
                    return ret;
                }
            }

            for (int i = 1; i != 3; i++)
            {
                // 4o-min is ~16x cheaper than 4o, thus we should try multiple times before resorting to
                // the high powered agent
                ret = await FromTextUsingAgentPunctuationStripped(text, StandardAgent);
                if (MaintainsInvariants(ret) & IsNonTrivial(ret))
                {
                    Debug.WriteLine($"successful decomposition after {i} attempts, with punctuation stripped");
                    return ret;
                }
            }

            //String text_stripped = Regex.Replace(text, @"\t|\n|\r", ",");


            // that failed, use more powerful agent (e.g. higher spec LLM)
            ret = await FromTextUsingAgent(text, HighPerformanceAgent);

            if (MaintainsInvariants(ret) & IsNonTrivial(ret))
            {
                Debug.WriteLine($"successful decomposition using high powered agent");
                return ret;
            }

            ret = await FromTextUsingAgent(text, StandardAgent);
            if (MaintainsInvariants(ret))
            {
                Debug.WriteLine($"successful decomposition, potentially trivial");
                return ret;
            }


            // do default, recommendation: use dummy agent guaranteed to maintain invariants
            ret = await FromTextUsingAgent(text, FallbackAgent);
            if (MaintainsInvariants(ret))
            {
                Debug.WriteLine($"had to resort to fall back agent for decomposition");
                return ret;
            }
            else
            {
                throw new Exception("Invalid decomposition");
            }
        }

        private async Task<TextDecomposition> FromTextUsingAgent(String text, AgentBase agent)
        {
            return TextDecomposition.FromNewLinedString(text, await agent.GetResponse(text), true);
        }

        private async Task<TextDecomposition> FromTextUsingAgentPunctuationStripped(String text, AgentBase agent)
        {
            String strippedText = StringHelper.StripPunctuation(text);
            return TextDecomposition.FromNewLinedString(text, await agent.GetResponse(strippedText), true);
        }

        public TextDecomposer()
        {
        }
    }
}

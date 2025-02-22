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
using Serilog;

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

            Func<TextDecomposition, bool> MaintainsInvariants = (TextDecomposition td)
                => (!mustBiject || td.Bijects()) && (!mustInject || td.Injects());

            Func<TextDecomposition, bool> IsNonTrivial = (TextDecomposition td)
                => (td.Decomposition?.Count ?? 1) > 1;

            // first pass at the decomposition, using StandardAgent

            TextDecomposition ret1;
            for (int i = 1; i != 5; i++)
            {
                // 4o-min is ~16x cheaper than 4o, thus we should try multiple times before resorting to
                // the high powered agent
                ret1 = await FromTextUsingAgent(text, StandardAgent);
                if (MaintainsInvariants(ret1) & IsNonTrivial(ret1))
                {
                    Debug.WriteLine($"successful decomposition after {i} attempts");
                    Log.Debug("Standard agent performed decomposition after {NumAttempts} attempts", i);
                    return ret1;
                }
            }

            TextDecomposition ret2;
            for (int i = 1; i != 3; i++)
            {
                // 4o-min is ~16x cheaper than 4o, thus we should try multiple times before resorting to
                // the high powered agent
                ret2 = await FromTextUsingAgentPunctuationStripped(text, StandardAgent);
                if (MaintainsInvariants(ret2) & IsNonTrivial(ret2))
                {
                    Log.Debug("Standard agent performed decomposition after {NumAttempts} attempts, with punctuation stripped", i);
                    return ret2;
                }
            }

            // that failed, use more powerful agent (e.g. higher spec LLM)
            TextDecomposition ret3 = await FromTextUsingAgent(text, HighPerformanceAgent);

            if (MaintainsInvariants(ret3) & IsNonTrivial(ret3))
            {
                Log.Information("had to use high powered agent for decomposition, hard text: \"{HardText}\"", text);
                return ret3;
            }

            if (MaintainsInvariants(ret3))
            {

                Log.Warning("successful decomposition using high powered agent, potentially trivial, hard text: \"{HardText}\"", text);
                return ret3;
            }


            // do default, recommendation: use dummy agent guaranteed to maintain invariants
            TextDecomposition ret4 = await FromTextUsingAgent(text, FallbackAgent);
            if (MaintainsInvariants(ret4))
            {
                Log.Warning("had to resort to fallback agent for decomposition, hard text: \"{HardText}\"", text);
                return ret4;
            }
            else
            {
                Log.Fatal("fallback agent was not able to maintain invariants, bad text: \"{BadText}\"", text);
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

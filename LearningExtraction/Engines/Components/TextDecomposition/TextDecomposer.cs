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
        private static List<char> ProblemChars = new List<char> { '\n', '\r' };

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
            // now have access to deepseek so can use different numbers of fallbacks

            TextDecomposition ret1;
            for (int i = 1; i != 2; i++) /* TODO - should this be a policy codified somewhere */
            {
                ret1 = await FromTextUsingAgent(text, StandardAgent);
                if (MaintainsInvariants(ret1) & IsNonTrivial(ret1))
                {
                    Debug.WriteLine($"successful decomposition after {i} attempts");
                    Log.Debug("Standard agent performed decomposition after {NumAttempts} attempts", i);
                    return ret1;
                }
            }

            // if that didn't work then lets just always uses the high powered agent
            // since it is rougly 2x more expensive, so not too much of a problem

            TextDecomposition? ret2 = null;
            for (int i = 1; i != 2; i++) /* TODO - should this be a policy codified somewhere */
            {
                ret2 = await FromTextUsingAgent(text, HighPerformanceAgent);
                if (MaintainsInvariants(ret2) & IsNonTrivial(ret2))
                {
                    Debug.WriteLine($"successful decomposition after {i} attempts");
                    Log.Debug("High performance agent performed decomposition after {NumAttempts} attempts", i);
                    return ret2;
                }
            }

            // TODO - keep an eye on the logs; if this isn't helping then remove it
            if (!mustBiject) /* stripping punctuation will definitiely impede bijectivity */
            {
                TextDecomposition ret3;
                for (int i = 1; i != 2; i++)
                {
                    ret3 = await FromTextUsingAgentPunctuationStripped(text, HighPerformanceAgent);
                    if (MaintainsInvariants(ret3) & IsNonTrivial(ret3))
                    {
                        Log.Debug("High performance agent performed decomposition after {NumAttempts} attempts, with punctuation stripped", i);
                        return ret3;
                    }
                }
            }

            if (ret2 is not null && MaintainsInvariants(ret2))
            {

                Log.Warning("successful decomposition using high powered agent, potentially trivial, hard text: \"{HardText}\"", text);
                return ret2;
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
            TextDecomposition ret = TextDecomposition.FromNewLinedString(text, await agent.GetResponse(text), true);

            // fixup weird /r/n newlines 
            ret = DecompositionHelper.ReintercalateMissingCharacters(ret, ProblemChars);

            return ret;
        }

        private async Task<TextDecomposition> FromTextUsingAgentPunctuationStripped(String text, AgentBase agent)
        {
            String strippedText = StringHelper.StripPunctuation(text);
            TextDecomposition ret = TextDecomposition.FromNewLinedString(text, await agent.GetResponse(strippedText), true);

            // fixup weird /r/n newlines 
            ret = DecompositionHelper.ReintercalateMissingCharacters(ret, ProblemChars);

            return ret;
        }


        public TextDecomposer()
        {
        }
    }
}

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
        // TODO - refactor this produce statements
        public int MaxVolumeToProcess { get; set; }     = 300; // if given text larger than this, chunk it
        public int JoinCharacterCount { get; set; }     = 20;
        public int PaddingCharacterCount { get; set; }  = 30; // longest English word is 45 letters 20+30=50

        public AgentBase StandardAgent { get; set; }
        public AgentBase HighPerformanceAgent { get; set; }
        public AgentBase FallbackAgent { get; set; }

        public async Task<TextDecomposition> DecomposeText(TextualMedia textSource, bool mustInject = true, bool mustBiject = false)
        {
            if (textSource.Text.Length > MaxVolumeToProcess)
            {
                // TODO - how to ensure that these will be valid values??
                List<String> windows = TextualMediaHelper.Windowed(textSource, MaxVolumeToProcess, JoinCharacterCount, PaddingCharacterCount);

                // Asynchronously decompose each window
                var decompositionTasks = windows.Select(window =>
                    DecomposeText(new TextualMedia { 
                                    Text = window,
                                    Description = textSource.Description },
                                  mustInject,
                                  mustBiject));

                // Wait for all decompositions to complete
                TextDecomposition[] partialDecompositions = await Task.WhenAll(decompositionTasks);

                // Combine partial decompositions
                return Combine(partialDecompositions.ToList(), JoinCharacterCount, PaddingCharacterCount, textSource, mustInject, mustBiject);
            }

            String newLinedDecomposition = await StandardAgent.GetResponse(textSource.Text); // only called in the base case
            TextDecomposition ret = TextDecomposition.FromNewLinedString(textSource.Text, newLinedDecomposition);

            if ((mustBiject && !ret.Bijects()) || (mustInject && !ret.Injects()))
            {
                // first attempt failed, use something more powerful
                String highPoweredAttempt = await HighPerformanceAgent.GetResponse(textSource.Text);
                ret = TextDecomposition.FromNewLinedString(textSource.Text, highPoweredAttempt);

                if ((mustBiject && !ret.Bijects()) || (mustInject && !ret.Injects()))
                {
                    // do default
                    String finalAttempt = await FallbackAgent.GetResponse(textSource.Text);
                    ret = TextDecomposition.FromNewLinedString(textSource.Text, finalAttempt);

                    if ((mustBiject && !ret.Bijects()) || (mustInject && !ret.Injects()))
                    {
                        throw new Exception("Invalid decomposition");
                    }
                }
            }
            return ret;
        }

        private TextDecomposition Combine(List<TextDecomposition> partialDecompositions, int joinCharacterCount, int paddingCharacterCount,
            TextualMedia parent, bool mustInject, bool mustBiject)
        {
            // base case 
            if (partialDecompositions.Count == 1)
            {
                return partialDecompositions[0];   
            }

            if (partialDecompositions.Count == 0)
            {
                throw new Exception("empty list");
            }

            if (mustBiject)
            {
                throw new NotImplementedException();
            }

            // combine first and second

            TextDecomposition lhs = partialDecompositions[0].Copy();
            TextDecomposition rhs = partialDecompositions[1].Copy();

            // trim padding around join
            // operates cleanly to leave no truncated units
            lhs = StripEndPadding(  lhs, paddingCharacterCount);
            rhs = StripStartPadding(rhs, paddingCharacterCount);

            int numberOfUnitsOverlap = DetermineOverlap(lhs.Decomposition, rhs.Decomposition);

            // remove the overlap regions from the lhs to avoid double inclusion
            for (int i = 0; i != numberOfUnitsOverlap; i++)
            {
                String toRemove = lhs.Decomposition.Last().Total;
                int removeIndex = lhs.Total.LastIndexOf(toRemove);

                lhs.Decomposition.Remove(lhs.Decomposition.Last());
                
                lhs = new TextDecomposition(
                    new String(lhs.Total.Substring(0, removeIndex)), lhs.Decomposition);
            }

            // combine the first and second
            TextDecomposition both = new TextDecomposition(new String(parent.Text), lhs.Copy().Decomposition);
            both.Decomposition.AddRange(rhs.Copy().Decomposition);

            for (int i = 1; mustInject && !both.Injects(); i++)
            {
                both = new TextDecomposition(new String(parent.Text), lhs.Copy().Decomposition);
                both.Decomposition.AddRange(rhs.Copy().Decomposition.Skip(i));

                if (i == rhs.Decomposition.Count)
                {
                    throw new Exception("failed to merge while maintaining injectivity");
                }
            }

            // create a new set of decompositions using the combined term
            List<TextDecomposition> updated = new List<TextDecomposition> { both };
            updated.AddRange(partialDecompositions.Skip(2));

            // recurse
            return Combine(updated, joinCharacterCount, paddingCharacterCount, parent, mustInject, mustBiject);
        }

        private int DetermineOverlap(List<TextDecomposition>? lhs, List<TextDecomposition>? rhs)
        {
            int ret = 0;

            if ((lhs?.Count ?? 0) == 0|| (rhs?.Count ?? 0) == 0)
            {
                return ret;
            }


            for (int i = 0; i < Math.Min(lhs.Count, rhs.Count); i++)
            {
                bool overlap = true;
                for (int j = 0; j != i; j++)
                {
                    overlap = overlap && lhs[lhs.Count - i + j].Total == rhs[j].Total;
                }
                if (overlap)
                {
                    ret = i;
                } // when the loop is done we'll get the biggest that worked

                // N^2 runtime --> watch out for slowdown if this handles big stuff
            }

            return ret;

        }

        private TextDecomposition StripEndPadding(TextDecomposition td, int paddingCharacterCount)
        {
            // removes all we can from the RHS padding section
            // guarantees retention of all non-padding text

            String remaining = new String(td.Total);
            List<TextDecomposition> remainingUnits = new List<TextDecomposition>(td.Decomposition);

            while (remainingUnits.Count > 0)
            {
                String unit = remainingUnits.Last().Total;

                int lastIndex = remaining.LastIndexOf(unit);

                if (td.Total.Length - lastIndex > paddingCharacterCount)
                {
                    // would remove characters not in the padding
                    break;
                } 
                else
                {
                    remaining = remaining.Substring(0, lastIndex);
                    remainingUnits.Remove(remainingUnits.Last());
                }
            }

            return new TextDecomposition(new String(remaining), remainingUnits);
        }

        private TextDecomposition StripStartPadding(TextDecomposition td, int paddingCharacterCount)
        {
            // removes all we can from the LHS padding section
            // guarantees retention of all non-padding text

            String remaining = new String(td.Total);
            int removed = 0;
            List<TextDecomposition> remainingUnits = new List<TextDecomposition>(td.Decomposition);

            while (remainingUnits.Count > 1)
            {
                String unit = remainingUnits[1].Total;

                int firstIndex = remaining.IndexOf(unit);

                if (firstIndex + removed > paddingCharacterCount)
                {
                    // would remove characters not in the padding
                    break;
                }
                else
                {
                    remaining = remaining.Substring(firstIndex);
                    remainingUnits.Remove(remainingUnits.First());
                    removed += firstIndex;
                }
            }

            return new TextDecomposition(new String(remaining), remainingUnits);
        }

        public TextDecomposer()
        {
        }
    }
}

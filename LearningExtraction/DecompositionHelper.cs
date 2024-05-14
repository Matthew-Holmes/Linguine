using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public static class DecompositionHelper
    {
        internal static Tuple<List<String>, int> Window(String original, int maxCharsToProcess, int joinLines)
        {
            // handles too large join sizes
            // has to return the join size in case it was reduced

            try
            {
                return Tuple.Create(WindowInternal(original, maxCharsToProcess, joinLines), joinLines);

            }
            catch (JoinException _)
            {
                return Window(original, maxCharsToProcess, joinLines / 2); // in edge case where the join lines were too many
            }
        }

        private static List<String> WindowInternal(String original, int maxCharsToProcess, int joinLines)
        {
            // base case
            if (original.Count() <= maxCharsToProcess)
            {
                return new List<String> { original };
            }

            List<String> ret = new List<String>();

            int rightSlice = maxCharsToProcess;
            int leftSlice;

            // make the first window

            while (original[rightSlice] != '\n' && rightSlice > 0)
            {
                rightSlice--;
            }

            rightSlice--; // consume the newline
            leftSlice = rightSlice;

            // form the join
            for (int i = 0; i != joinLines; i++)
            {
                while (leftSlice > 0 && original[leftSlice] != '\n')
                {
                    leftSlice--;
                }
                leftSlice--; // consume the newline
            }

            if (leftSlice <= 0)
            {
                // join too big
                throw new JoinException();
            }

            ret.Add(original.Substring(0, rightSlice + 1));

            // recurse
            ret.AddRange(WindowInternal(original.Substring(leftSlice + 2), maxCharsToProcess, joinLines));

            return ret;
        }

        private class JoinException : Exception
        { };
    }
}

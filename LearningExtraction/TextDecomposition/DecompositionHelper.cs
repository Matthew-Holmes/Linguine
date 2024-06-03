using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public static class DecompositionHelper
    {
        #region windowing
        internal static Tuple<List<String>, int> Window(TextDecomposition td, int maxCharsToProcess, int joinLines)
        {
            return Window(td.NewLinedUnitsString, maxCharsToProcess, joinLines);
        }

        internal static Tuple<List<String>, int> Window(String newLinedString, int maxCharsToProcess, int joinLines)
        {
            // handles too large join sizes
            // has to return the join size in case it was reduced

            try
            {
                return Tuple.Create(WindowInternal(newLinedString, maxCharsToProcess, joinLines), joinLines);

            }
            catch (JoinException _)
            {
                return Window(newLinedString, maxCharsToProcess, joinLines / 2); // in edge case where the join lines were too many
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

        #endregion



        internal static List<int> GetUnitLocations(TextDecomposition injective)
        {
            // TODO - write tests

            if (!injective.Injects())
            {
                throw new ArgumentException("provided decomposition must inject");
            }

            int consumed = 0;
            List<int> ret = new List<int>();

            foreach (String unit in injective.Units)
            {
                int pos = injective.Total.Substring(consumed).IndexOf(unit);
                ret.Add(consumed + pos);

                consumed += pos;
            }

            return ret;
        }

        internal static List<String> GetContextWindows(TextDecomposition injective, int LHSContextUnits, int RHSContextUnits, int MaxChars)
        {
            if (!injective.Injects())
            {
                throw new ArgumentException("provided decomposition must inject");
            }

            List<String> ret = new List<String>();

            List<int> unitLocations = GetUnitLocations(injective);
            unitLocations.Add(injective.Total.Length); // off the end location

            int lhsUnit = 0;
            int rhsUnit = Math.Min(RHSContextUnits, injective.Units.Count);

            int lhsTmp, rhsTmp;

            for (int i = 0; i != injective.Units.Count; i++)
            {
                // advance the context window
                while (lhsUnit < i - LHSContextUnits)
                {
                    lhsUnit++;
                }
                                                                 //index of last true unit location entry - we added an off the end index too               
                while (rhsUnit < i + RHSContextUnits && rhsUnit < unitLocations.Count - 2 )
                {
                    rhsUnit++;
                }


                // get the substring forming the context
                int len = unitLocations[rhsUnit + 1] - unitLocations[lhsUnit];
                String toAdd = injective.Total.Substring(unitLocations[lhsUnit], len);

                // ensure meets the max character constraints
                lhsTmp = lhsUnit;
                rhsTmp = rhsUnit;

                while (toAdd.Length > MaxChars && lhsTmp != rhsTmp)
                {
                    // contract the window

                    if (lhsTmp != i)
                    {
                        lhsTmp++;
                    }

                    int lenTmp = unitLocations[rhsTmp+1] - unitLocations[lhsTmp];
                    toAdd = injective.Total.Substring(unitLocations[lhsTmp], lenTmp);

                    if (toAdd.Length <= MaxChars) { break; }

                    // contract at the opposite end
                    if (rhsTmp != i)
                    {
                        rhsTmp--;
                    }

                    lenTmp = unitLocations[rhsTmp + 1] - unitLocations[lhsTmp];
                    toAdd = injective.Total.Substring(unitLocations[lhsTmp], lenTmp);
                }

                if (toAdd.Length > MaxChars)
                {
                    toAdd = ""; // the unit itself is too big, just skip it
                }

                ret.Add(toAdd);

            }

            return ret;
        }

    }
}

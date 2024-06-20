using ExternalMedia;
using Infrastructure;
using LearningExtraction;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public static class DecompositionHelper
    {
        public static List<int> GetUnitLocations(TextDecomposition injective)
        {
            // TODO - write tests

            if (!injective.Injects())
            {
                throw new ArgumentException("provided decomposition must inject");
            }

            if (injective.Units is null)
            {
                return new List<int>();
            }

            int consumed = 0;
            List<int> ret = new List<int>();

            foreach (String unit in injective.Units)
            {
                int pos = injective.Total.Substring(consumed).IndexOf(unit);
                ret.Add(consumed + pos);

                consumed += pos + unit.Length;
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

            if (unitLocations.Count == 0) { return new List<string>(); }

            unitLocations.Add(injective.Total.Length); // off the end location

            int lhsUnit = 0;
            int rhsUnit = Math.Min(RHSContextUnits, injective.Units.Count - 1);

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

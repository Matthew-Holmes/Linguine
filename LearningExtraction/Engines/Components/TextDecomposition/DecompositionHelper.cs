using Serilog;
using System.Collections.Immutable;
using System.Text;
using DataClasses;

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

        internal static TextDecomposition ReintercalateMissingCharacters(TextDecomposition ret, List<char> problemChars, bool bijects = false)
        {
            // this will only work well for truly injective/almost bijection decompositions
            // but then, those are the only ones it needs to work well for!

            String parent = ret.Total;
            // string.Join(" ", parent.Select(c => ((int)c).ToString())) // in watch window to check the ascii
            if (ret.Decomposition is null)
            {
                Log.Warning("trying to intercalate problem chars on empty text decomposition {parent}", parent);
                return ret;
            }

            int parent_ptr = 0;

            List<TextDecomposition> fixedUnits = new List<TextDecomposition>();

            foreach (TextDecomposition unit in ret.Decomposition)
            {
                if (unit.Decomposition is not null)
                {
                    throw new Exception("ReintercalateMissingCharacters called on non suitable (not shallow) text decomposition!");
                }

                String        unit_text   = unit.Total;
                StringBuilder newUnitText = new StringBuilder();

                int unit_ptr = 0;

                while (unit_ptr < unit_text.Length && parent_ptr < parent.Length)
                {
                    while (parent_ptr < parent.Length && unit_text[unit_ptr] != parent[parent_ptr])
                    {
                        parent_ptr++; // there might be gaps in the decomposition (injectivity)
                    }

                    if (parent_ptr == parent.Length)
                        break;

                    if (unit_text[unit_ptr] == parent[parent_ptr])
                    {
                        newUnitText.Append(unit_text[unit_ptr]);
                        unit_ptr++; parent_ptr++;
                    }

                    while (parent_ptr < parent.Length && problemChars.Contains(parent[parent_ptr]))
                    {
                        char toIntercalate = parent[parent_ptr];
                        newUnitText.Append(toIntercalate);

                        // Log.Information("reintercalated problem char in unit: {unit}", unit_text);

                        parent_ptr++;

                        if (unit_ptr < unit_text.Length && unit_text[unit_ptr] == ' ')
                        {
                            unit_ptr++; // sometimes the parent uses newlines in lieu of spaces, this handles that
                        }
                    }
                }

                if (!bijects) /* if bijective we want to keep everything */
                {
                    // reverse iterate stripping the problem chars from the unit again 
                    for (int i = newUnitText.Length - 1; i >= 0; i--)
                    {
                        if (problemChars.Contains(newUnitText[i]))
                        {
                            newUnitText.Remove(i, 1);
                            if (i == 0)
                            {
                                Log.Warning("had a unit entirely composed of problem characters in {text}", parent);
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                // only works for leaves
                fixedUnits.Add(new TextDecomposition(newUnitText.ToString(), null)); // TODO - make an AsLeaf method??
            }

            return new TextDecomposition(parent, fixedUnits);
        }
    }
}

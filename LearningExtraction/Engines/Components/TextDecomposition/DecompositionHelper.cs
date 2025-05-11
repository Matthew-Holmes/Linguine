using Serilog;
using System.Collections.Immutable;
using System.Text;
using DataClasses;
using System.Buffers;

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

        // TODO - switch to rune


    internal static TextDecomposition ReintercalateMissingCharacters(
        TextDecomposition ret,
        List<Rune> problemRunes,
        bool bijects = false)
    {
        // this will only work well for truly injective/almost bijection decompositions
        // but then, those are the only ones it needs to work well for!

        string parent = ret.Total;

        if (ret.Decomposition is null)
        {
            Log.Warning("trying to intercalate problem chars on empty text decomposition {parent}", parent);
            return ret;
        }

        // --- helpers -----------------------------------------------------------
        static List<Rune> ToRunes(string s)
        {
            var runes = new List<Rune>(s.Length);         // over‑allocate slightly
            ReadOnlySpan<char> span = s.AsSpan();

            for (int i = 0; i < span.Length;)
            {
                OperationStatus status = Rune.DecodeFromUtf16(span.Slice(i), out Rune r, out int consumed);
                if (status != OperationStatus.Done)
                {
                    throw new InvalidOperationException("Invalid UTF-16 sequence detected.");
                }
                runes.Add(r);
                i += consumed;
            }

            return runes;
        }
        // -----------------------------------------------------------------------

        List<Rune> parentRunes = ToRunes(parent);
        int parent_ptr = 0;
        List<TextDecomposition> fixedUnits = new();

        foreach (TextDecomposition unit in ret.Decomposition)
        {
            if (unit.Decomposition is not null)
                throw new Exception("ReintercalateMissingCharacters called on non suitable (not shallow) text decomposition!");

            List<Rune> unitRunes = ToRunes(unit.Total);
            StringBuilder newUnitText = new StringBuilder();

            int unit_ptr = 0;

            while (unit_ptr < unitRunes.Count && parent_ptr < parentRunes.Count)
            {
                while (parent_ptr < parentRunes.Count && !unitRunes[unit_ptr].Equals(parentRunes[parent_ptr]))
                {
                    parent_ptr++;                                   // there might be gaps in the decomposition (injectivity)
                }

                if (parent_ptr == parentRunes.Count)
                    break;

                if (unitRunes[unit_ptr].Equals(parentRunes[parent_ptr]))
                {
                    newUnitText.Append(unitRunes[unit_ptr].ToString());
                    unit_ptr++; parent_ptr++;
                }

                while (parent_ptr < parentRunes.Count && problemRunes.Contains(parentRunes[parent_ptr]))
                {
                    Rune toIntercalate = parentRunes[parent_ptr];
                    newUnitText.Append(toIntercalate.ToString());

                    // Log.Information("reintercalated problem char in unit: {unit}", unit_text);

                    parent_ptr++;

                    if (unit_ptr < unitRunes.Count && unitRunes[unit_ptr].Value == ' ')
                    {
                        unit_ptr++; // sometimes the parent uses newlines in lieu of spaces, this handles that
                    }
                }
            }

            if (!bijects) /* if bijective we want to keep everything */
            {
                // reverse iterate stripping the problem runes from the unit again
                // (convert the builder to a rune list so we trim whole scalars, not UTF‑16 code units)
                List<Rune> newRunes = ToRunes(newUnitText.ToString());

                for (int i = newRunes.Count - 1; i >= 0; i--)
                {
                    if (problemRunes.Contains(newRunes[i]))
                    {
                        newRunes.RemoveAt(i);
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

                newUnitText.Clear();
                foreach (Rune r in newRunes) newUnitText.Append(r.ToString());
            }

            // only works for leaves
            fixedUnits.Add(new TextDecomposition(newUnitText.ToString(), null)); // TODO - make an AsLeaf method??
        }

        return new TextDecomposition(parent, fixedUnits);
    }
}
}

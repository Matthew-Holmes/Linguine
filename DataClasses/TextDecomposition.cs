﻿using Newtonsoft.Json;

namespace DataClasses
{
    public class TextDecomposition
    {
        // a piece of text decomposed into units
        // a null value for Units means no further decomposition - an 'atom'

        [JsonProperty("T")]
        public String Total { get; private set; }

        [JsonIgnore]
        public DictionaryDefinition? Definition { get; set; }

        [JsonProperty("D")]
        public List<TextDecomposition>? Decomposition { get; private set; }

        [JsonIgnore]
        public List<String>? Units => Decomposition?.Select(x => x.Total).ToList() ?? null;

        // TODO - remove this in case when units have new lines in?
        [JsonIgnore]
        public String NewLinedUnitsString => Units is not null ? String.Join('\n', Units) : "";

        public TextDecomposition(String total, List<TextDecomposition>? decomposition)
        {
            Total = total;
            Decomposition = decomposition;
        }

        public TextDecomposition Copy()
        {
            return new TextDecomposition(this.Total, Decomposition?.Select(unit => unit.Copy()).ToList() ?? null);
        }

        public bool Injects()
        {
            // checks if each unit exists in the total, without overlap with other units
            // this methods validates for the entire hierarchy

            if (Decomposition is null)
            {
                return true; // base case, leaf
            }

            String remaining = Total;

            foreach (TextDecomposition unit in Decomposition)
            {
                String toFind = unit.Total;
                if (!remaining.Contains(toFind) || !unit.Injects(/*recursion*/))
                {
                    return false;
                }
                else
                {
                    int toSnip = remaining.IndexOf(toFind) + toFind.Length;
                    remaining = remaining.Substring(toSnip);
                }
            }

            return true;
        }

        public bool Bijects()
        {
            if (Decomposition is null)
            {
                return true; // base case, leaf
            }

            // if every tree level injects, then any surplus Total would have to propagate up
            // therefore can apply the pigeonhole principle

            int count = Decomposition.Sum(td => td.Total.Length);
            return count == Total.Length && Injects();
        }

        public TextDecomposition Flattened()
        {
            if (Decomposition is null || Decomposition.Count <= 1) /* a leaf or at least should be */
            {
                return new TextDecomposition(Total, null);
                // TODO - should this attach definition info too??
            }
            else
            {
                List<TextDecomposition> flatUnits = new List<TextDecomposition>();

                foreach (TextDecomposition td in Decomposition)
                {
                    if (td.Decomposition is null)
                    {
                        flatUnits.Add(td); // already a leaf
                    }
                    else
                    {
                        flatUnits.AddRange(td.Flattened().Decomposition);
                    }
                }

                return new TextDecomposition(Total, flatUnits);
            }
        }

        public static TextDecomposition FromNewLinedString(String parent, String newLinedDecomposition, bool trim = false)
        {
            String total = parent;
            List<TextDecomposition> decomposition = new List<TextDecomposition>();

            foreach (String substring in newLinedDecomposition.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                String unit = new String(substring);
                if (trim)
                {
                    unit = unit.Trim();
                }
                decomposition.Add(new TextDecomposition(unit, null)); // leaves
            }

            if (decomposition.Count == 0)
            {
                return new TextDecomposition(total, null); // is a leaf
            }
            else if (decomposition.Count == 1 && decomposition.First().Total == parent)
            {
                return new TextDecomposition(total, null); // single element newLinedDecomposition bijects, is a leaf
            }
            else
            {
                return new TextDecomposition(total, decomposition);
            }
        }
    }
}

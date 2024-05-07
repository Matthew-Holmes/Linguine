using ExternalMedia;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public class TextDecomposition
    {
        // a piece of text decomposed into units
        // a null value for Units means no further decomposition - an 'atom'

        public TextUnit Total { get; private set; }

        public List<TextDecomposition>? Units { get; private set; }

        public TextDecomposition(TextUnit total, List<TextDecomposition>? units)
        {
            Total = total;
            Units = units;
        }

        public TextDecomposition Copy()
        {
            TextUnit totalCopy = new TextUnit(new String(this.Total.Text));

            return new TextDecomposition(totalCopy, Units?.Select(unit => unit.Copy()).ToList() ?? null);
        }

        public bool Injects()
        {
            // checks if each unit exists in the total, without overlap with other units
            // this methods validates for the entire hierarchy

            if (Units is null)
            {
                return true; // base case, leaf
            }

            String remaining = new String(Total.Text);

            foreach (TextDecomposition unit in Units)
            {
                String toFind = unit.Total.Text;
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
            if (Units is null)
            {
                return true; // base case, leaf
            }

            // if every tree level injects, then any surplus Total would have to propagate up
            // therefore can apply the pigeonhole principle

            int count = Units.Sum(td => td.Total.Text.Length);
            return count == Total.Text.Length && Injects();
        }

        public TextDecomposition Flattened()
        {
            if (Units is null || Units.Count <= 1) /* a leaf or at least should be */
            {
                return new TextDecomposition(Total, null);
            }
            else
            {
                List<TextDecomposition> flatUnits = new List<TextDecomposition>();

                foreach (TextDecomposition td in Units)
                {
                    if (td.Units is null)
                    {
                        flatUnits.Add(td); // already a leaf
                    }
                    else
                    {
                        flatUnits.AddRange(td.Flattened().Units);
                    }
                }

                return new TextDecomposition(Total, flatUnits);
            }
        }


        internal static TextDecomposition FromNewLinedString(String parent, String newLinedDecomposition)
        {
            TextUnit total = new TextUnit(parent);
            List<TextDecomposition> decomposition = new List<TextDecomposition>();

            foreach (String substring in newLinedDecomposition.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            {
                TextUnit unit = new TextUnit(substring);
                decomposition.Add(new TextDecomposition(unit, null)); // leaves
            }

            if (decomposition.Count == 0)
            {
                return new TextDecomposition(total, null); // is a leaf
            }
            else if (decomposition.Count == 1 && decomposition.First().Total.Text == parent)
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

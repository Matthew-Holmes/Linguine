using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public static class PercentageHelper
    {
        public static List<decimal> RoundDistinctPercentages(List<decimal> percentages)
        {
            // Step 1: Round to the nearest integer
            var rounded = percentages.Select(p => Math.Round(p)).ToList();

            // Step 2: Check for collisions
            var distinctRounded = rounded.Distinct().Count() == rounded.Count;

            // If no collisions, return the rounded percentages as decimals
            if (distinctRounded)
            {
                return rounded.Select(p => Convert.ToDecimal(p)).ToList();
            }

            // Dictionary to hold the final rounded values
            var finalRounded = new List<decimal>();

            // Step 4: Process each percentage
            for (int i = 0; i < percentages.Count; i++)
            {
                int precision = 0;

                decimal current = percentages[i];
                decimal roundedValue = Math.Round(current, precision);
                
                while (finalRounded.Contains(roundedValue) && precision <= 25) // Max precision for decimal is 25 if in 0,100
                {
                    precision++;

                    // step down the precision of all colliding elements
                    int j = finalRounded.FindIndex(v => v == roundedValue);
                    int prog = j+1; // so we don't get stuck in infinite loops
                    while (j != -1)
                    {
                        finalRounded[j] = Math.Round(percentages[j], precision);
                        j = finalRounded.FindIndex(prog,v => v == roundedValue);
                        prog = j+1;
                    }

                    roundedValue = Math.Round(current, precision);
                }

                finalRounded.Add(roundedValue);
            }

            return finalRounded;
        }
    }
}

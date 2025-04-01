using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public static class SelectionSampler
    {
        public static List<T> Sample<T>(IList<T> input, int k, Random rng)
        {
            k = Math.Min(k, input.Count);

            if (k == 0)
            {
                return new List<T>();
            }

            List<T> result = new List<T>(k);

            int needed = k;
            int left = input.Count;

            foreach (var item in input)
            {
                double probability = (double)needed / left;
                if (rng.NextDouble() < probability)
                {
                    result.Add(item);
                    needed--;
                    if (needed == 0)
                        break;
                }
                left--;
            }

            return result;
        }
    }

}

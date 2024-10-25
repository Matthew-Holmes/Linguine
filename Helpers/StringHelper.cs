using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Helpers
{
    public static class StringHelper
    {
        public static string StripPunctuation(String s)
        {
            var sb = new StringBuilder();
            foreach (char c in s)
            {
                if (!char.IsPunctuation(c))
                    sb.Append(c);
            }
            return sb.ToString();
        }

        public static string StripOuterPunctuation(String s)
        {
            int start = 0;
            int end = s.Length - 1;

            while (start <= end && char.IsPunctuation(s[start]))
                start++;

            while (end >= start && char.IsPunctuation(s[end]))
                end--;

            return s.Substring(start, end - start + 1);
        }
    }
}

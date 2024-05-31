using Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalMedia
{
    public static class TextualMediaHelper
    {
      
        public static List<String> Windowed(TextualMedia tm, int windowWidth, int joinChars, int padChars)
        {
            // chunks up the Text into overlapping windows
            // joinChars is the number of overlap characters
            // then add further padding, padChars, to either side
            // the first and last window have asymmetry -
            // since include the start and end of the text
            // and thus don't need padding/joins there

            return Window(tm.Text, windowWidth, joinChars, padChars);
        }

        public static List<String> Window(String parent, int windowWidth, int joinChars, int padChars)
        {
            int coreChars = windowWidth - 2 * joinChars - 2 * padChars;


            #region argument checking
            if (coreChars < 0)
            {
                throw new ArgumentException("core must have non-negative width");
            }
            if (joinChars < 0)
            {
                throw new ArgumentException("join must have non-negative width");
            }
            if (padChars < 0)
            {
                throw new ArgumentException("padding must have non-negative width");
            }

            if (coreChars + joinChars <= 0)
            {
                // otherwise chunking will never reduce the total
                throw new ArgumentException("core and join must have content");
            }
            #endregion

            if (parent.Length <= coreChars) return new List<String> { parent };

            List<String> ret = new List<String>();

            // no padding/join at the front
            String firstWindow = parent.Substring(0, Math.Min(coreChars + joinChars + padChars, parent.Length));
            ret.Add(firstWindow);

            int nextCoreStarts = coreChars + joinChars;

            // iterates until processes penultimate window
            while (nextCoreStarts < parent.Length - coreChars - joinChars)
            {
                ret.Add(parent.Substring(
                    Math.Max(nextCoreStarts - joinChars - padChars, 0),
                    Math.Min(coreChars + 2 * joinChars + 2 * padChars,
                             Math.Min(parent.Length - nextCoreStarts + joinChars + padChars,
                                      parent.Length /* edge case where max used the 0 branch */))));

                nextCoreStarts += (coreChars + joinChars);
            }

            // no padding/join at the back
            String lastWindow = parent.Substring(
                Math.Max(nextCoreStarts - joinChars - padChars, 0),
                         Math.Min(parent.Length - nextCoreStarts + joinChars + padChars,
                                  parent.Length /* edge case where max used the 0 branch */));

            ret.Add(lastWindow);

            return ret;
        }
    }
}

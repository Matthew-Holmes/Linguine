using Infrastructure;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExternalMedia
{
    public class TextualMedia
    {
        String _text;
        String _description;
        LanguageCode _languageCode;

        public TextualMedia(string text, LanguageCode lc, string description = "")
        {
            _text = text;
            _description = description;
            _languageCode = lc;
        }

        public String Text
        {
            get => _text;
        }

        public String Description
        {
            get => _description;
        }

        public LanguageCode LanguageCode
        {
            get => _languageCode;
        }

        public List<String> Windowed(int coreChars, int joinChars, int padChars)
        {
            // chunks up the Text into overlapping windows
            // joinChars is the number of overlap characters
            // then add further padding, padChars, to either side
            // the first and last window have asymmetry -
            // since include the start and end of the text
            // and thus don't need padding/joins there

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

            if (Text.Length <= coreChars) return new List<String> { Text };

            List<String> ret = new List<String>();

            // no padding/join at the front
            String firstWindow = Text.Substring(0, Math.Min(coreChars + joinChars + padChars, Text.Length));
            ret.Add(firstWindow);

            int nextCoreStarts = coreChars + joinChars;

            // iterates until processes penultimate window
            while (nextCoreStarts < Text.Length - coreChars - joinChars)
            {
                ret.Add(Text.Substring(
                    Math.Max(nextCoreStarts - joinChars - padChars, 0),
                    Math.Min(coreChars + 2*joinChars + 2*padChars,
                             Math.Min(Text.Length - nextCoreStarts + joinChars + padChars,
                                      Text.Length /* edge case where max used the 0 branch */))));

                nextCoreStarts += (coreChars + joinChars);
            }

            // no padding/join at the back
            String lastWindow = Text.Substring(
                Math.Max(nextCoreStarts - joinChars - padChars, 0),
                         Math.Min(Text.Length - nextCoreStarts + joinChars + padChars,
                                  Text.Length /* edge case where max used the 0 branch */));

            ret.Add(lastWindow);

            return ret;
        }
    }
}

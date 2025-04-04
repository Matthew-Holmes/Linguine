using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace DataClasses
{
    public class DictionaryDefinition
    {
        [Key]
        public int DatabasePrimaryKey { get; set; }
        public int ID { get; set; }
        public String Word { get; set; }
        public String Definition { get; set; }
        public String Source { get; set; }
        public String? IPAPronunciation { get; set; }
        public String? RomanisedPronuncation { get; set; }


        
        public string GetSafeFileName()
        {
            var sanitizedChars = Word
                    .Select(c =>
                        char.IsLetterOrDigit(c) || CharUnicodeInfo.GetUnicodeCategory(c) == UnicodeCategory.OtherLetter
                            ? c
                            : char.IsWhiteSpace(c) ? '_' : '\0' /* placeholder to remove */
                    )
                    .Where(c => c != '\0')
                    .ToArray();

            string sanitized = new string(sanitizedChars);

            sanitized = sanitized.Replace(' ', '_');

            string shortPart = sanitized.Length > 20 ? sanitized.Substring(0, 20) : sanitized;

            if (shortPart.Length > 0)
            {
                shortPart += '_';
            }

            return $"{shortPart}{DatabasePrimaryKey}".ToLowerInvariant();
        }

    }

}

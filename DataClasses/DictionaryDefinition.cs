using System.ComponentModel.DataAnnotations;

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
    }
}

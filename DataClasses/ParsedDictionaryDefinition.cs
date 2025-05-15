using System.ComponentModel.DataAnnotations;

namespace DataClasses
{
    public class ParsedDictionaryDefinition
    {
        [Key]
        public int DatabasePrimaryKey { get; set; }
        public int DictionaryDefinitionKey { get; set; }
        public DictionaryDefinition CoreDefinition { get; set; }
        public LearnerLevel LearnerLevel { get; set; }
        public LanguageCode NativeLanguage { get; set; }   
        public string ParsedDefinition { get; set; }

        public EntryMethod ParsedDefinitionEntryMethod { get; set; } = EntryMethod.Machine;
    }
}

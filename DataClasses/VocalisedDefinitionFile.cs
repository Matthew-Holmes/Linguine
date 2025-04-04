using System.ComponentModel.DataAnnotations;

namespace DataClasses
{
    public class VocalisedDefinitionFile
    {
        [Key]
        public int                  DatabasePrimaryKey      { get; set; }
        public int                  DictionaryDefinitionKey { get; set; }
        public DictionaryDefinition Definition              { get; set; }
        public Voice                Voice                   { get; set; }
        public decimal              Speed                   { get; set; } = 1.0m; // this is not settable yet in the google api for the voices I want :(
        public String               FileName                { get; set; }
    }
}

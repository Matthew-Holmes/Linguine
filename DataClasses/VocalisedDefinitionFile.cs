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
        public decimal              Speed                   { get; set; }
        public String               FileName                { get; set; }
    }
}

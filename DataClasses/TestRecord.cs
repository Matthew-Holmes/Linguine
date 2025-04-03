using System.ComponentModel.DataAnnotations;

namespace DataClasses
{
    public class TestRecord
    {
        [Key]
        public int                  DatabasePrimaryKey      { get; set; }
        public int                  DictionaryDefinitionKey { get; set; }
        public DictionaryDefinition Definition              { get; set; }
        public DateTime             Posed                   { get; set; }
        public DateTime             Answered                { get; set; }
        public DateTime             Finished                { get; set; }
        public bool                 Correct                 { get; set; }
    }
}

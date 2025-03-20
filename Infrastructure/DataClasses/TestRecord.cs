using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataClasses
{
    // TODO - should these be parsed dictionary definitions if second language??
    public class TestRecord
    {
        [Key]
        public int                  DatabasePrimaryKey      { get; set; }
        public int                  DictionaryDefinitionKey { get; set; }
        public DictionaryDefinition Definition              { get; set; }
        public DateTime             Posed                   { get; set; }
        public DateTime             Answered                { get; set; }
        public DateTime             Finished                { get; set; }
    }
}

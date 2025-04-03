using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataClasses
{
    class VocalisedDefinitionFile
    {
        [Key]
        public int                  DatabasePrimaryKey      { get; set; }
        public int                  DictionaryDefinitionKey { get; set; }
        public DictionaryDefinition Definition              { get; set; }
        public                      Voice Voice             { get; set; }
        public String               FileName                { get; set; }
    }
}

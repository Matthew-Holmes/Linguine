using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
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
    }
}

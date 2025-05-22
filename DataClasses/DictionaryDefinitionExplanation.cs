using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataClasses
{
    public class DictionaryDefinitionExplanation
    {
        [Key]
        public int DatabasePrimaryKey { get; set; }
        public int DictionaryDefinitionKey { get; set; }
        public DictionaryDefinition CoreDefinition { get; set; }
        public LanguageCode NativeLanguage { get; set; }
        public String Explanation { get; set; }

        public EntryMethod ExplanationEntryMethod { get; set; } = EntryMethod.Machine;
    }
}

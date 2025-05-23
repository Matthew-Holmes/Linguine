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
        public String DetailedDefinition { get; set; } // may include structures that are specific to this word
        public String Synonyms { get; set; }
        public String DifferencesFromOtherSenses { get; set; }
        public String Register { get; set; } // tone/register/specific to specific domain
        public String Example1 { get; set; }
        public String Example2 { get; set; }
        public String Example3 { get; set; }
        public EntryMethod DetailedDefinitionEntryMethod { get; set; } = EntryMethod.Machine;
        public EntryMethod SynonymsEntryMethod { get; set; } = EntryMethod.Machine;
        public EntryMethod DifferencesEntryMethod { get; set; } = EntryMethod.Machine;
        public EntryMethod RegisterEntryMethod { get; set; } = EntryMethod.Machine;
        public EntryMethod Example1EntryMethod { get; set; } = EntryMethod.Machine;
        public EntryMethod Example2EntryMethod { get; set; } = EntryMethod.Machine;
        public EntryMethod Example3EntryMethod { get; set; } = EntryMethod.Machine;
    }
}

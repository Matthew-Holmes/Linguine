using System.ComponentModel.DataAnnotations;
using DataClasses;

namespace Infrastructure
{
    public enum EntryMethod
    {
        Machine = 0,
        User = 1,
        UserOverwriteMachine = 2,
        UserOverwriteUser = 3
    }

    public class StatementDefinitionNode
    {
        [Key]
        public int DatabasePrimaryKey { get; set; }

        public int CurrentLevel { get; set; } // 0 --> root, then 1, 2, ..etc
        public int IndexAtCurrentLevel { get; set; } // left to right

        public int DefinitionKey { get; set; }
        public DictionaryDefinition? DictionaryDefinition { get; set; } // null if no definition here

        public int StatementKey { get; set; }
        public StatementDatabaseEntry StatementDatabaseEntry { get; set; }

        public EntryMethod WasManuallyEntered { get; set; } = EntryMethod.Machine;
    }
}

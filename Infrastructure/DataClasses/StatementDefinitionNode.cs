using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
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
    }
}

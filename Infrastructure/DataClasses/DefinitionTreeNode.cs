using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataClasses
{
    public class DefinitionTreeNode
    {
        [Key]
        public int DatabasePrimaryKey { get; set; }
        public int IndexAtCurrentLevel { get; set; }
        public int DefinitionKey { get; set; }
        public DictionaryDefinition? DictionaryDefinition { get; set; } // null if no definition here
        public int ParentKey { get; set; }
        public DefinitionTreeNode? Parent { get; set; } // null if root node
    }
}

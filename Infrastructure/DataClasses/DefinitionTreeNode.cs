using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataClasses
{
    public class DefinitionTreeNode
    {
        public int IndexAtCurrentLevel { get; set; }
        public DictionaryDefinition? DictionaryDefinition { get; set; } // null if no definition here
        public DefinitionTreeNode? Parent { get; set; } // null if root node
    }
}

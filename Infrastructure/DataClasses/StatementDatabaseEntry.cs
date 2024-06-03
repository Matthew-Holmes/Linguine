using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataClasses
{
    public class StatementDatabaseEntry
    {
        // since these will be numerous and data rich, use a minimal entry to store in the database
        [Key]
        public int DatabasePrimaryKey { get ; set; }

        // statement slice identification
        public TextualMedia Parent { get; set; }
        public int ParentKey { get; set; }
        public int FirstCharIndex { get; set; }
        public int LastCharIndex { get; set; }

        // efficient storing of contextual information
        public StatementDatabaseEntry? Previous { get; set; } // null if the first statement of a piece of media
        public int PreviousKey { get; set; }
        public List<String>? ContextCheckpoint { get; set; } // so don't have to replay all the way to root
        public List<int> ContextDeltaRemovalsDescendingIndex { get; set; }
        public List<Tuple<int, String>> ContextDeltaInsertionsDescendingIndex { get; set; }


        // compressed TextDecompositions
        public String HeadlessInjectiveDecompositionJSON { get; set; }
        public String HeadlessRootedDecompositionJSON { get; set; }
        public int DefinitionTreeRootKey { get; set; }  
        public DefinitionTreeNode DefinitionTreeRoot { get; set; }
    }
}

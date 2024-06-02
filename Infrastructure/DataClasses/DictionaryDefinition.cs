using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class DictionaryDefinition
    {
        [Key]
        public int DatabasePrimaryKey { get; set; }
        public int ID { get; set; }
        public String Word { get; set; }
        public String Definition { get; set; }
        public String Source { get; set; }
    }
}

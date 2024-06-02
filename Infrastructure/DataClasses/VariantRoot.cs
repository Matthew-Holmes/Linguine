using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class VariantRoot
    {
        [Key]
        public int DatabasePrimaryKey { get; set; }
        public String Variant { get; set; }
        public String Root { get; set; }
        public String Source { get; set; }  
    }
}

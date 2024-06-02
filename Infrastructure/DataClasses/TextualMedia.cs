using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class TextualMedia
    {
        public string Name { get; set; }    
        public string Text { get; set; }
        public string Description { get; set; }
        [Key]
        public int DatabasePrimaryKey { get; set; }
    }
}

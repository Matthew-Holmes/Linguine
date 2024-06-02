using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class TextualMediaSession
    {
        [Key]
        public int DatabasePrimaryKey { get; set; }
        public int TextualMediaKey { get; set; }
        public TextualMedia TextualMedia { get; set; }
        public int Cursor { get; set; }
        public bool Active { get; set; }
        public DateTime LastActive{ get; set; }
    }
}

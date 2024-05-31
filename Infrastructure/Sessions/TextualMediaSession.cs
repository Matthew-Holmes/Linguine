using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public class TextualMediaSession
    {
        public int DatabasePrimaryKey { get; set; }
        public string FilestoreLocation { get; set; }
        public int Cursor { get; set; }
        public bool Active { get; set; }
    }
}

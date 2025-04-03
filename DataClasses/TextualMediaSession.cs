using System.ComponentModel.DataAnnotations;

namespace DataClasses
{
    public class TextualMediaSession
    {
        [Key]
        public int DatabasePrimaryKey { get; set; }
        public int TextualMediaKey { get; set; }
        public virtual TextualMedia TextualMedia { get; set; }
        public int Cursor { get; set; }
        public bool Active { get; set; }
        public DateTime LastActive{ get; set; }
    }
}

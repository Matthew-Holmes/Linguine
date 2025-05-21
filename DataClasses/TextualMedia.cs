using System.ComponentModel.DataAnnotations;

namespace DataClasses
{
    public class TextualMedia
    {
        public string Name { get; set; }    
        public string Text { get; set; }
        public string Description { get; set; }
        [Key]
        public int DatabasePrimaryKey { get; set; }
        public bool IsOpen { get; set; } = false;
    }
}

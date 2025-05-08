using System.ComponentModel.DataAnnotations;

namespace DataClasses
{
    public class VariantRoot
    {
        [Key]
        public int DatabasePrimaryKey { get; set; }
        public String Variant { get; set; }
        public String Root { get; set; }
    }
}

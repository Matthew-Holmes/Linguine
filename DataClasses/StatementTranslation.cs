using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataClasses
{
    public class StatementTranslation
    {
        [Key]
        public int DatabasePrimaryKey { get; set; }
        public Statement Underlying { get; set; } // no foreign key, since this goes via the anticorruption layer
        public LanguageCode TranslatedLanguage { get; set; }
        public String Translation { get; set; }
    }
}

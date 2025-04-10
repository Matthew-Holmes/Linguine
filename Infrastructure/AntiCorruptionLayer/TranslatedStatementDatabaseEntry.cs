using DataClasses;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.AntiCorruptionLayer
{
    public class TranslatedStatementDatabaseEntry
    {
        [Key]
        public int DatabasePrimaryKey { get; set; }
        public int StatementKey { get; set; }
        public StatementDatabaseEntry StatementDatabaseEntry { get; set; }
        public LanguageCode TranslatedLanguage { get; set; }
        public String Translation { get; set; }
    }
}

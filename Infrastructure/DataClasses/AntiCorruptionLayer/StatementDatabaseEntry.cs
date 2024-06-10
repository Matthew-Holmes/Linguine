using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class StatementDatabaseEntry
    {
        // since these will be numerous and data rich, use a minimal entry to store in the database
        [Key]
        public int DatabasePrimaryKey { get ; set; }

        // statement slice identification
        public TextualMedia Parent { get; set; }
        public int ParentKey { get; set; }
        public int FirstCharIndex { get; set; }
        public int LastCharIndex { get; set; }

        // efficient storing of contextual information
        public StatementDatabaseEntry? Previous { get; set; } // null if the first statement of a piece of media
        public int? PreviousKey { get; set; }
        public List<String>? ContextCheckpoint { get; set; } // so don't have to replay all the way to root
        public List<int> ContextDeltaRemovalsDescendingIndex { get; set; }
        public List<Tuple<int, String>> ContextDeltaInsertionsDescendingIndex { get; set; }


        // compressed TextDecompositions
        public String HeadlessInjectiveDecompositionJSON { get; set; }
        public String HeadlessRootedDecompositionJSON { get; set; }
    }

    public class InsertionsJSONConverter : ValueConverter<List<Tuple<int, string>>, string>
    {
        public InsertionsJSONConverter() : base(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<List<Tuple<int, string>>>(v))
        { }
    }

    public class RemovalsJSONConverter : ValueConverter<List<int>, string>
    {
        public RemovalsJSONConverter() : base(
            v => JsonConvert.SerializeObject(v),
            v => JsonConvert.DeserializeObject<List<int>>(v))
        { }
    }

    public class ContextJSONConverter : ValueConverter<List<String>?, string>
    {
        public ContextJSONConverter() : base(
            v => JsonConvert.SerializeObject(v, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            }),
            v => JsonConvert.DeserializeObject<List<String>?>(v, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            }))
        { }
    }
}

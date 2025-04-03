using System.Reflection;
using DataClasses;

namespace LearningExtraction
{ 
    public class ProtoStatementBuilder
    {
        // class to store intermediary data members required for the Statement class

        public String? StatementText { get; set; } = null;
        public List<String>? Context { get; set; } = null;
        public TextDecomposition? InjectiveDecomposition { get; set; } = null;
        public TextDecomposition? RootedDecomposition { get; set; } = null;

        public ProtoStatement ToProtoStatement()
        {
            if (HasAnyNullProperties(this))
            {
                throw new Exception("haven't provided all properties");
            }
            return new ProtoStatement(StatementText, Context, InjectiveDecomposition, RootedDecomposition);
        }

        private static bool HasAnyNullProperties(ProtoStatementBuilder obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return properties.Any(prop => prop.GetValue(obj) == null);
        }
    }
}

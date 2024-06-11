using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public class StatementBuilder
    {
        // class to store intermediary data members required for the Statement class

        public TextualMedia?        Parent                  { get; set; }   = null;
        public String?              StatementText           { get; set; } = null;
        public List<String>?        Context                 { get; set; } = null;
        public TextDecomposition?   InjectiveDecomposition  { get; set; } = null;
        public TextDecomposition?   RootedDecomposition     { get; set; } = null;
        public int?                 FirstCharIndex          { get;set; } = null;
        public int?                 LastCharIndex           { get; set; } = null;

        public Statement ToStatement()
        {
            if (HasAnyNullProperties(this))
            {
                throw new Exception("haven't provided all properties");
            }
            return new Statement(Parent, (int)FirstCharIndex, (int)LastCharIndex, StatementText, 
                Context, InjectiveDecomposition, RootedDecomposition);
        }

        private static bool HasAnyNullProperties(StatementBuilder obj)
        {
            PropertyInfo[] properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            return properties.Any(prop => prop.GetValue(obj) == null);
        }
    }
}

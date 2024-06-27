using Agents;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.DataClasses.AgentInteractionStore
{
    internal class AgentQuery
    {
        [Key]
        public int                  DatabasePrimaryKey      { get; set; }

        public String               Prompt                  { get; set;  }
        public String               Response                { get; set; }
        public int                  AgentParametrisationKey { get; set; }
        public AgentParametrisation AgentParametrisation    { get; set; }
        public LLM                  LLM                     { get; set; }
        public AgentTask            AgentTask               { get; set; }
        public LanguageCode         Language                { get; set; }

        // if false then bad, null implies unknown
        public bool?                IsGoodExample           { get; set; }
        public String?              ExtraInfo               { get; set; } // use for custom tagging
    }
}

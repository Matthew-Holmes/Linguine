﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure 
{
    // lightweight class to facilitate tagging of queries with the agent that handled it
    // also means can produce reports on performance without having to instantiate fully
    // fledged ParametrisedAgentBase instances that require API connections/keys
    public class AgentParametrisation
    {
        [Key]
        public int          DatabasePrimaryKey { get; set; }
        public String       JSONParameters     { get; set; } // this is a serialised ParametrisedAgentBase
        public String       Hash               { get; set; } 
    }
}

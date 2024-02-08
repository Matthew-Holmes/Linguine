using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public abstract class ParametrisedAgentBase : AgentBase
    {
        public List<Parameter<double>>    ContinousParameters { get; } = new List<Parameter<double>>();
        public List<Parameter<int>>       DiscreteParameters  { get; } = new List<Parameter<int>>();
        public Dictionary<String, String> StringParameters    { get; } = new Dictionary<String, String>();


        public Parameter<double> ContinousParameter(String name)
        {
            return ContinousParameters.Where(p => p.Name == name).First();            
        }

        public Parameter<int> DiscreteParameter(String name)
        {
            return DiscreteParameters.Where(p => p.Name == name).First();
        }

    }
}

using Helpers;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public abstract class ParametrisedAgentBase : AgentBase
    {
        public List<Parameter<double>>    ContinuousParameters { get; } = new List<Parameter<double>>();
        public List<Parameter<int>>       DiscreteParameters   { get; } = new List<Parameter<int>>();
        public Dictionary<String, String> StringParameters     { get; } = new Dictionary<String, String>();


        public Parameter<double> ContinuousParameter(String name)
        {
            return ContinuousParameters.Where(p => p.Name == name).First();            
        }

        public Parameter<int> DiscreteParameter(String name)
        {
            return DiscreteParameters.Where(p => p.Name == name).First();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static T FromJson<T>(string json) where T : ParametrisedAgentBase
        {
            return JsonConvert.DeserializeObject<T>(json);
        }
    }
}

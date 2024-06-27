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

        public ParametrisedAgentBase(SemaphoreSlim? globalSemaphore = null) : base(globalSemaphore) { }

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

        public override int GetHashCode()
        {
            int hash = 17; // initial start

            foreach (var param in ContinuousParameters)
            {
                hash = hash * 31 + param.Name.GetHashCode();
                hash = hash * 31 + param.Value.GetHashCode();
            }

            foreach (var param in DiscreteParameters)
            {
                hash = hash * 13 + param.Name.GetHashCode();
                hash = hash * 13 + param.Value.GetHashCode();
            }

            foreach (var kvp in StringParameters)
            {
                hash = hash * 19 + kvp.Key.GetHashCode();
                hash = hash * 19 + kvp.Value.GetHashCode();
            }

            return hash;
        }
    }
}

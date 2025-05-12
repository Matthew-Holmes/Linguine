using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning
{
    public static class DefinitionFrequencyEngineFactory
    {
        public static IDefinitionFrequencyEngine BuildDefinitionFrequencyEngine()
        {
            return new DefinitionFrequencyEngine();
        }
    }
}

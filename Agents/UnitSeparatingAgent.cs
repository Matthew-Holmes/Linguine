using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class UnitSeparatingAgent : OpenAIProcessingBase
    {
        private static String systemMessage = "consider the following lines, separate lines with multiple words, UNLESS they are TRUE compound words found in a dictionary; break up compound nouns unless they are very common such as \"hot dog\", names etc. Put each part on a new line. DO NOT alter any content. format \nword1\nword2 etc";
        
        public UnitSeparatingAgent(string apiKey) : base(apiKey)
        {
            StringParameters["system"] = systemMessage;
        }
    }
}

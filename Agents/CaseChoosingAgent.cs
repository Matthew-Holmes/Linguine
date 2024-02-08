using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class CaseChoosingAgent : OpenAIBase
    {
        private static String preamble = "Given each line of options, choose the one that is most commonly used for that line in the language, ignoring the order of the lines or options input format: line1var1\tline1var2...\nline2var1\nline2var1...\n... output: line1varx\nline2vary\n.... Do not omit lines.";
        public CaseChoosingAgent(string apiKey) : base(apiKey, preamble, 0, 1000, "gpt-3.5-turbo-0125", 0.1m) { }
    }
}

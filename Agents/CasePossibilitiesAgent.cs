using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class CasePossibilitiesAgent : OpenAIBase
    {
        private static String preamble = "for each line provided, produce possibile variants, changing upper and lower case letters. If that variant is valid for the meaning of the line, ignoring setence structure and punctuation, add it to the list, input format: line1\nline2\n... return format: line1var1\tline1var2...\nline2var1\tline2var1...\n... use tabs between variants options. Do not omit lines. Example in: \"The\nquick\nred\nfox\nAdam\njump\n.\nHe\njump\nhight\" out: \"The\tthe\nquick\nred\nfox\nAdam\njump\n.\nHe\nhe\njump\nhigh\"";
        public CasePossibilitiesAgent(string apiKey) : base(apiKey, preamble, 0, 1000, "gpt-3.5-turbo-0125", 1.0m) { }

    }
}

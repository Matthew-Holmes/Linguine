using Agents.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public class RootChooserAgent : OpenAIBase
    {
        //private static String preamble = "Select from the options below the correct root of the original, considering the context, choose lowercase (if possible), infinitive and singular forms. Reply ONLY with the number of the correct option, e.g. \"n\" where n an integer DO NOT include any textual characters. If none are correct, reply 0";
        private static String preamble = "You will be provided with options to choose from, reply ONLY with a single integer corresponding to the correct option, nothing else. Given an original word/phrase choose the correct root, considering the context. If none of the options are correct reply 0";
        public RootChooserAgent(string apiKey) : base(apiKey, preamble, 0, 1000) { }

    }
}

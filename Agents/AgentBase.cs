using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public abstract class AgentBase
    {
        public AgentBase()
        {
            PromptResponseHistory = new List<Tuple<String, String>>();
        }

        private object responseLock = new object();

        public String GetResponse(String prompt)
        {
            String response;

            lock (responseLock)
            {
                response = GetResponseCore(prompt);

                PromptResponseHistory.Add(Tuple.Create(prompt, response));

            }
            return response;
        }

        protected abstract String GetResponseCore(String prompt);

        public List<Tuple<String, String>> PromptResponseHistory { get; private set; }
    }
}

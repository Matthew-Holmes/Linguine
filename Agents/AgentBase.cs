using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public abstract class AgentBase
    {
        public List<Tuple<String, String>> PromptResponseHistory { get; private set; } = new List<Tuple<String, String>>();

        private SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1); // use this instead of lock since doing async stuff

        public async Task<String> GetResponse(String prompt)
        {
            String response;
            //await _semaphore.WaitAsync(); // get permission

            try
            {
                response = await GetResponseCore(prompt).ConfigureAwait(false); // since this isn't a UI thing we'll let it resume on any thread TODO - understand this better!
                PromptResponseHistory.Add(Tuple.Create(prompt, response));
            } finally
            {
               // _semaphore.Release();
            }

            return response;
        }

        protected abstract Task<String> GetResponseCore(String prompt);
    }
}

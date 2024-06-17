using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public abstract class AgentBase
    {
        private static int _concurrencyLimit = 30;

        public List<Tuple<String, String>>          SequentialPromptLog { get; private set; } 
            = new List<Tuple<String, String>>();
        public ConcurrentBag<Tuple<String, String>> ConcurrentPromptLog { get; private set; } 
            = new ConcurrentBag<Tuple<String, String>>();

        private SemaphoreSlim _semaphore = new SemaphoreSlim(1,_concurrencyLimit); 
        // use this instead of lock since doing async stuff

        private int _maxConcurrentResponses = 1;
        public int MaxConcurrentResponses
        {
            get => _maxConcurrentResponses;
            set
            {
                if (value > _concurrencyLimit)
                {
                    MaxConcurrentResponses = _concurrencyLimit;
                }
                else if (value < MaxConcurrentResponses)
                {
                    while (_semaphore.CurrentCount > value)
                    {
                        _semaphore.Wait();
                    }
                } else if (value > MaxConcurrentResponses)
                {
                    while (_semaphore.CurrentCount < value)
                    {
                        _semaphore.Release();
                    }
                }
            }
        }


        public async Task<String> GetResponse(String prompt)
        {
            String response;
            await _semaphore.WaitAsync(); // get permission

            try
            {
                response = await GetResponseCore(prompt).ConfigureAwait(false); 
                // since this isn't a UI thing we'll let it resume on any thread TODO - understand this better!

                if (MaxConcurrentResponses == 1)
                {
                    SequentialPromptLog.Add(Tuple.Create(prompt, response));
                }
                else
                {
                    ConcurrentPromptLog.Add(Tuple.Create(prompt, response));
                }
            } finally
            {
               _semaphore.Release();
            }

            return response;
        }

        protected abstract Task<String> GetResponseCore(String prompt);
    }
}

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Infrastructure;

namespace Agents
{
    public abstract class AgentBase
    {
        public AgentBase(SemaphoreSlim? globalSemaphore = null)
        {
            // to inhibit access to a single LLM API for all agents that use it
            _globalSemaphore = globalSemaphore;
        }
        private SemaphoreSlim? _globalSemaphore;

        private static int _concurrencyLimit = 30;

        [JsonIgnore]
        public AgentTask    AgentTask { get; set; }
        [JsonIgnore]
        public LLM          LLM       { get; set; }
        [JsonIgnore]
        public LanguageCode Language  { get; set; }

        [JsonIgnore]
        public List<Tuple<String, String>>          SequentialPromptLog { get; private set; } 
            = new List<Tuple<String, String>>();
        // used for agents which use conversation history for context

        private SemaphoreSlim _semaphore = new SemaphoreSlim(1,_concurrencyLimit); 
        // use this instead of lock since doing async stuff

        private int _maxConcurrentResponses = 1;

        [JsonIgnore]
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
            await _semaphore.WaitAsync(); // get local permission
            
            if (_globalSemaphore is not null)
            {
                await _globalSemaphore.WaitAsync();
            }

            try
            {
                response = await GetResponseCore(prompt).ConfigureAwait(false); 
                // since this isn't a UI thing we'll let it resume on any thread TODO - understand this better!

                if (MaxConcurrentResponses == 1)
                {
                    SequentialPromptLog.Add(Tuple.Create(prompt, response));
                }
            } finally
            {
                // even if call throws don't upset concurrency limits
               _globalSemaphore?.Release();
               _semaphore.Release();
            }

            return response;
        }

        public override int GetHashCode()
        {
            return GetType().Name.GetHashCode();
        }

        protected abstract Task<String> GetResponseCore(String prompt);
    }
}

using Agents.OpenAI;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{

    public class MissingAPIKeyException : Exception
    {
        public MissingAPIKeyException()
        { }

        public MissingAPIKeyException(string message)
            : base(message)
        { }

        public MissingAPIKeyException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }

    public static class AgentFactory
    {
        private static SemaphoreSlim OpenAISemaphore = new SemaphoreSlim(10); // global API lock for OpenAI

 
        public static AgentBase GenerateProcessingAgent(
            API_Keys keys,
            AgentTask task,
            LanguageCode language,
            bool isHighPerformace = false)
        {
            LLM model = LLM.ChatGPT4o_mini;

            if (isHighPerformace)
            {
                model = LLM.ChatGPT4o;
            }

            return GenerateProcessingAgentInternal(keys, task, language, model);
        }

        private static AgentBase GenerateProcessingAgentInternal(
            API_Keys keys,
            AgentTask task,
            LanguageCode language,
            LLM model = LLM.ChatGPT4o_mini)
        {
            if (model == LLM.ChatGPT3_5 || model == LLM.ChatGPT4o || model == LLM.ChatGPT4o_mini)
            {
                if (keys.OpenAI_APIKey is null)
                {
                    throw new MissingAPIKeyException("missing open ai API key!");
                }
                return GenerateOpenAIProcessingAgent(keys.OpenAI_APIKey, task, language, model);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static AgentBase GenerateOpenAIProcessingAgent(
            String key, AgentTask task, 
            LanguageCode language, LLM model)
        {
            OpenAIBase ret = new OpenAIBase(key, OpenAISemaphore);

            // processing, so no history and allows concurrency
            ret.DiscreteParameter("PromptDepth").Value = 0;
            ret.MaxConcurrentResponses = 5; // local concurrency limit

            ret.StringParameters["system"] = SystemMessageFactory.SystemMessageFor(task, language);

            if (model == LLM.ChatGPT3_5)
            {
                ret.StringParameters["model"] = "gpt-3.5-turbo-0125";

                ret.DiscreteParameter("ContextTokens").Value  = 16000;
                ret.DiscreteParameter("ResponseTokens").Value = 4000;

            } else if (model == LLM.ChatGPT4o)
            {
                ret.StringParameters["model"] = "gpt-4o";

                ret.DiscreteParameter("ContextTokens").Value  = 128000;
                ret.DiscreteParameter("ResponseTokens").Value = 4000;
            } else if (model == LLM.ChatGPT4o_mini)
            {
                ret.StringParameters["model"] = "gpt-4o-mini";
                ret.DiscreteParameter("ContextTokens").Value  = 128000;
                ret.DiscreteParameter("ResponseTokens").Value = 4000;
            }
            else
            {
                throw new NotImplementedException();
            }

            // TODO - employed agent 
            //ret.AgentTask = task;
            //ret.LLM       = model;
            //ret.Language  = language;

            return ret;
        }
    }
}

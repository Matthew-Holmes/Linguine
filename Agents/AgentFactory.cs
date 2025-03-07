using Agents.DeepSeek;
using Agents.OpenAI;
using Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
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
        public string missingLocation;

        public MissingAPIKeyException(string missingLocation)
        {
            this.missingLocation = missingLocation;
        }

        public MissingAPIKeyException(string missingLocation, string message)
            : base(message)
        {
            this.missingLocation = missingLocation;
        }

        public MissingAPIKeyException(string missingLocation, string message, Exception innerException)
            : base(message, innerException)
        {
            this.missingLocation = missingLocation;
        }
    }

    public static class AgentFactory
    {
        private static SemaphoreSlim OpenAISemaphore   = new SemaphoreSlim(10); // global API lock for OpenAI
        private static SemaphoreSlim DeepSeekSemaphore = new SemaphoreSlim(10); // "" deepseek
 
        public static AgentBase GenerateProcessingAgent(
            AgentTask task,
            LanguageCode language,
            bool isHighPerformance = false)
        {
            LLM model = LLM.ChatGPT4o_mini;

            if (isHighPerformance)
            {
                model = LLM.DeepSeek_chat; // cheaper than 4o (and mayber even cheaper than mini in off hours!)
            }

            return GenerateProcessingAgentInternal(task, language, model);
        }

        private static AgentBase GenerateProcessingAgentInternal(
            AgentTask task, LanguageCode language, LLM model = LLM.ChatGPT4o_mini)
        {
            if (model == LLM.ChatGPT3_5 || model == LLM.ChatGPT4o || model == LLM.ChatGPT4o_mini)
            {
                return GenerateOpenAIProcessingAgent(task, language, model);
            }
            else if (model == LLM.DeepSeek_chat)
            {

                return GenerateDeepSeekProcessingAgent(task, language, model);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private static AgentBase GenerateDeepSeekProcessingAgent(
            AgentTask task, LanguageCode language, LLM model)
        {
            String? apiKey = ConfigManager.APIKeys.DeepSeek_APIKey;

            if (apiKey is null)
            {
                throw new MissingAPIKeyException(
                    ConfigManager.Config.APIKeys.DeepSeek_APIKeyLocation,
                    "missing deepseek API key!");
            }

            DeepSeekBase ret = new DeepSeekBase(apiKey, DeepSeekSemaphore);

            // processing, so no history and allows concurrency
            ret.DiscreteParameter("PromptDepth").Value = 0;
            ret.MaxConcurrentResponses = 5; // local concurrency limit

            ret.StringParameters["system"] = SystemMessageFactory.SystemMessageFor(task, language);

            if (model == LLM.DeepSeek_chat)
            {
                ret.StringParameters["model"] = "deepseek-chat";

                ret.DiscreteParameter("ContextTokens").Value = 16000; // TODO - what are these
                ret.DiscreteParameter("ResponseTokens").Value = 4000; // TODO - ""

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

        private static AgentBase GenerateOpenAIProcessingAgent(
            AgentTask task, LanguageCode language, LLM model)
        {
            String? apiKey = ConfigManager.APIKeys.OpenAI_APIKey;

            if (apiKey is null)
            {
                throw new MissingAPIKeyException(
                    ConfigManager.Config.APIKeys.OpenAI_APIKeyLocation,
                    "missing open ai API key!");
            }

            OpenAIBase ret = new OpenAIBase(apiKey, OpenAISemaphore);

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

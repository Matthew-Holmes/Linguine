﻿using Agents.OpenAI;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Agents
{
    public static class AgentFactory
    {
        private static SemaphoreSlim OpenAISemaphore = new SemaphoreSlim(10); // global API lock for OpenAI

        public static AgentBase GenerateProcessingAgent(
            String key,
            AgentTask task,
            LanguageCode language,
            LLM model = LLM.ChatGPT3_5)
        {
            if (model == LLM.ChatGPT3_5 || model == LLM.ChatGPT4o)
            {
                return GenerateOpenAIProcessingAgent(key, task, language, model);
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
            } else
            {
                throw new NotImplementedException();
            }

            ret.AgentTask = task;
            ret.LLM       = model;
            ret.Language  = language;

            return ret;
        }
    }
}

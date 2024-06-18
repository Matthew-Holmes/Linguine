using Agents;
using Agents.DummyAgents;
using Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public static class TextDecomposerFactory
    {
        public static TextDecomposer MakeStatementsDecomposer(string openAI_APIKey, LanguageCode lc)
        {
            TextDecomposer ret = new TextDecomposer();

            ret.MaxVolumeToProcess    = 5000;
            ret.PaddingCharacterCount = 500;
            ret.JoinCharacterCount    = 500;

            ret.StandardAgent        = AgentFactory.GenerateProcessingAgent(openAI_APIKey, AgentTask.DecompositionToStatements, lc);

            ret.HighPerformanceAgent = AgentFactory.GenerateProcessingAgent(openAI_APIKey, AgentTask.DecompositionToStatements, lc /*, LLM.ChatGPT4o (too expensive)*/);

            ret.FallbackAgent        = new SentenceDecompositionAgent();

            return ret;
        }

        public static TextDecomposer MakeUnitsDecomposer(string openAI_APIKey, LanguageCode lc)
        {
            TextDecomposer ret = new TextDecomposer();

            ret.MaxVolumeToProcess    = 5000;
            ret.PaddingCharacterCount = 20;
            ret.JoinCharacterCount    = 30; // longest English word is 45 letters 20+30=50

            ret.StandardAgent       = AgentFactory.GenerateProcessingAgent(openAI_APIKey,
                AgentTask.DecompositionToUnits, lc);

            ret.HighPerformanceAgent = AgentFactory.GenerateProcessingAgent(openAI_APIKey,
                AgentTask.DecompositionToUnits, lc);

            ret.FallbackAgent        = new WhitespaceDecompositionAgent();

            return ret;
        }
    }
}

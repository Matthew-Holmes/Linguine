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
        public static TextDecomposer MakeStatementsDecomposer(API_Keys keys, LanguageCode lc)
        {
            TextDecomposer ret = new TextDecomposer();

            ret.MaxVolumeToProcess = (int)(5000 * LanguageCodeDetails.Density(lc));

            ret.StandardAgent        = AgentFactory.GenerateProcessingAgent(keys, AgentTask.DecompositionToStatements, lc);

            ret.HighPerformanceAgent = AgentFactory.GenerateProcessingAgent(keys, AgentTask.DecompositionToStatements, lc ,LLM.ChatGPT4o);

            ret.FallbackAgent        = new SentenceDecompositionAgent();

            return ret;
        }

        public static TextDecomposer MakeUnitsDecomposer(API_Keys keys, LanguageCode lc)
        {
            TextDecomposer ret = new TextDecomposer();

            ret.MaxVolumeToProcess = (int)(5000 * LanguageCodeDetails.Density(lc));

            ret.StandardAgent       = AgentFactory.GenerateProcessingAgent(keys,
                AgentTask.DecompositionToUnits, lc);

            ret.HighPerformanceAgent = AgentFactory.GenerateProcessingAgent(keys,
                AgentTask.DecompositionToUnits, lc ,LLM.ChatGPT4o);

            ret.FallbackAgent        = new WhitespaceDecompositionAgent();

            return ret;
        }
    }
}

using Agents;
using Config;
using DataClasses;
using LearningExtraction.Engines.DefinitionExplainingEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public static class DefinitionExplainerFactory
    {
        public static ICanExplainDefinitions BuildExplanationEngine()
        {
            AgentBase explainingAgent = AgentFactory.GenerateProcessingAgent(
                AgentTask.DefinitionExplaining,
                ConfigManager.Config.Languages.NativeLanguage,
                isHighPerformance: true);

            return new DefinitionExplainingEngine(explainingAgent);
        }
    }
}

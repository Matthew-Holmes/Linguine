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
            AgentBase detailedDefinitionAgent = AgentFactory.GenerateProcessingAgent(
                AgentTask.DetailedDefinitionWriting,
                ConfigManager.Config.Languages.NativeLanguage,
                isHighPerformance: true);

            AgentBase synonymsAgent = AgentFactory.GenerateProcessingAgent(
                AgentTask.DefinitionSynonyms,
                ConfigManager.Config.Languages.NativeLanguage,
                isHighPerformance: true);

            AgentBase wordSenseDistinguishingAgent = AgentFactory.GenerateProcessingAgent(
                AgentTask.WordSenseDistinguishingExplanation,
                ConfigManager.Config.Languages.NativeLanguage,
                isHighPerformance: true);

            AgentBase registerAgent = AgentFactory.GenerateProcessingAgent(
                AgentTask.RegisterAnalysis,
                ConfigManager.Config.Languages.NativeLanguage,
                isHighPerformance: true);

            AgentBase easyExampleAgent = AgentFactory.GenerateProcessingAgent(
                AgentTask.SimpleExampleOfWord,
                ConfigManager.Config.Languages.TargetLanguage);

            AgentBase mediumExampleAgent = AgentFactory.GenerateProcessingAgent(
                AgentTask.MediumExampleOfWord,
                ConfigManager.Config.Languages.TargetLanguage);

            AgentBase hardExampleAgent = AgentFactory.GenerateProcessingAgent(
                AgentTask.HardExampleOfWord,
                ConfigManager.Config.Languages.TargetLanguage,
                isHighPerformance: true);

            var ret = new DefinitionExplainingEngine();

            ret.DetailedAgent         = detailedDefinitionAgent;
            ret.SynonymAgent          = synonymsAgent;
            ret.DistinguishingAgent   = wordSenseDistinguishingAgent;
            ret.RegisterAnalysisAgent = registerAgent;
            ret.ExampleAgent1         = easyExampleAgent;
            ret.ExampleAgent2         = mediumExampleAgent;
            ret.ExampleAgent3         = hardExampleAgent;

            return ret;
        }
    }
}

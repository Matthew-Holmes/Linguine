using Agents;
using Config;
using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public static class InteractiveDefinitionResolutionEngineFactory
    {
        public static ICanResolveDefinitions BuildDefinitionResolutionEngine()
        {
            // use native language since that adds system prompt "you are a language learning assistant" if user is say, English
            AgentBase generalAgent = AgentFactory.GenerateProcessingAgent(
                AgentTask.GeneralPurpose, 
                ConfigManager.Config.Languages.NativeLanguage,
                isHighPerformance: true);

            return new InteractiveDefinitionResolutionEngine(generalAgent);
        }
    }
}

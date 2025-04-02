using Agents;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{

    public static class DefinitionPronouncerFactory
    {
        public static ICanPronounce BuildPronunciationEngine()
        {
            AgentBase IPAAgent = AgentFactory.GenerateProcessingAgent(
                AgentTask.DefinitionIPAPronouncing,
                ConfigManager.Config.Languages.TargetLanguage);

            AgentBase romaniseAgent = AgentFactory.GenerateProcessingAgent(
                AgentTask.DefinitionRomanisedPronouncing,
                ConfigManager.Config.Languages.TargetLanguage);

            return new DefinitionPronunciationEngine(IPAAgent, romaniseAgent);
        }
    }
}

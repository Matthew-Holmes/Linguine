using Agents;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public static class DefinitionParserFactory
    {
        public static ICanParseDefinitions BuildParsingEngine()
        {
            AgentBase parsingAgent = AgentFactory.GenerateProcessingAgent(
                AgentTask.DefinitionParsing, ConfigManager.Config.Languages.NativeLanguage);

            return new DefinitionParsingEngine(parsingAgent);
        }
    }
}

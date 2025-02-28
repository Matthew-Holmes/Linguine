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
        public static ICanParseDefinitions BuildParsingEngine(API_Keys keys, LanguageCode nativeLanguage)
        {
            AgentBase parsingAgent = AgentFactory.GenerateProcessingAgent(
                keys, AgentTask.DefinitionParsing, ConfigManager.NativeLanguage);

            return new DefinitionParsingEngine(parsingAgent);

        }
    }
}

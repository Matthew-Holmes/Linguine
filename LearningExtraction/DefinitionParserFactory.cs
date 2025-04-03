using Agents;
using DataClasses;
using Config;

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

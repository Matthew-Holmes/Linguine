using Agents;
using Agents.DummyAgents;
using DataClasses;
using Config;

namespace LearningExtraction
{
    public static class TextDecomposerFactory
    {
        public static TextDecomposer MakeStatementsDecomposer()
        {
            LanguageCode lc = ConfigManager.Config.Languages.TargetLanguage;

            TextDecomposer ret = new TextDecomposer();

            ret.MaxVolumeToProcess = (int)(5000 * LanguageCodeDetails.Density(lc));

            ret.StandardAgent        = AgentFactory.GenerateProcessingAgent(
                AgentTask.DecompositionToStatements, lc);

            ret.HighPerformanceAgent = AgentFactory.GenerateProcessingAgent(
                AgentTask.DecompositionToStatements, lc, true);

            ret.FallbackAgent        = new SentenceDecompositionAgent();

            return ret;
        }

        public static TextDecomposer MakeUnitsDecomposer()
        {
            LanguageCode lc = ConfigManager.Config.Languages.TargetLanguage;

            TextDecomposer ret = new TextDecomposer();

            ret.MaxVolumeToProcess = (int)(5000 * LanguageCodeDetails.Density(lc));

            ret.StandardAgent       = AgentFactory.GenerateProcessingAgent(
                AgentTask.DecompositionToUnits, lc);

            ret.HighPerformanceAgent = AgentFactory.GenerateProcessingAgent(
                AgentTask.DecompositionToUnits, lc, true);

            ret.FallbackAgent        = new WhitespaceDecompositionAgent();

            return ret;
        }
    }
}

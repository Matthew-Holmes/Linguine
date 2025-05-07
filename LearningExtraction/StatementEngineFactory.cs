using Agents;
using Infrastructure;
using DataClasses;
using Config;
using Helpers;

namespace LearningExtraction
{
    public static class StatementEngineFactory
    {
        public static StatementEngine BuildStatementEngine(ExternalDictionary dictionary)
        {
            LanguageCode targetLanguage = ConfigManager.Config.Languages.TargetLanguage;

            StatementEngine ret = new StatementEngine();

            ret.ToStatementsDecomposer   = TextDecomposerFactory.MakeStatementsDecomposer();
            ret.FromStatementsDecomposer = TextDecomposerFactory.MakeUnitsDecomposer();

            // these aren't actually used any more!
            ret.ContextChangeIdentificationAgent = AgentFactory.GenerateProcessingAgent(
                AgentTask.ContextChangeIdentification, targetLanguage);

            ret.ContextUpdateAgent = AgentFactory.GenerateProcessingAgent(
                AgentTask.ContextUpdating, targetLanguage);

            ret.UnitRooter = new UnitRooter();
            ret.UnitRooter.Agent = AgentFactory.GenerateProcessingAgent(
                AgentTask.UnitRooting, targetLanguage, true);

            // keep this on the cheaper API for now (?) since is the most expensive part
            ret.DefinitionResolver = new BatchDefinitionResolver();

            ret.DefinitionResolver.MultiDefinitionAgent  = AgentFactory.GenerateProcessingAgent(
                AgentTask.MultiDefinitionResolution, targetLanguage);
            ret.DefinitionResolver.SingleDefinitionAgent = AgentFactory.GenerateProcessingAgent(
                AgentTask.SingleDefinitionResolution, targetLanguage);

            ret.DefinitionResolver.PromptParts  = TextFactory.DefinitionResolutionString(targetLanguage);
            ret.DefinitionResolver.Affirmatives = TextFactory.Affirmatives(targetLanguage);
            ret.DefinitionResolver.Negatives    = TextFactory.Negatives(targetLanguage);

            ret.DefinitionResolver.Dictionary = dictionary; // maybe make an interface for what the dictionary is used for here

            return ret;
        }
    }
}

using Agents;
using Infrastructure;
using LearningExtraction;
using Microsoft.EntityFrameworkCore.Metadata;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    internal static class StatementEngineFactory
    {
        public static StatementEngine BuildStatementEngine(String? open_ai_apiKey, ExternalDictionary dictionary)
        {
            StatementEngine ret = new StatementEngine();

            ret.ToStatementsDecomposer = TextDecomposerFactory.MakeStatementsDecomposer(open_ai_apiKey, 
                ConfigManager.TargetLanguage);
            ret.FromStatementsDecomposer = TextDecomposerFactory.MakeUnitsDecomposer(open_ai_apiKey, ConfigManager.TargetLanguage);

            ret.ContextChangeIdentificationAgent = AgentFactory.GenerateProcessingAgent(
                open_ai_apiKey, AgentTask.ContextChangeIdentification, ConfigManager.TargetLanguage);

            ret.ContextUpdateAgent = AgentFactory.GenerateProcessingAgent(
                open_ai_apiKey, AgentTask.ContextUpdating, ConfigManager.TargetLanguage);

            ret.UnitRooter = new UnitRooter();
            ret.UnitRooter.Agent = AgentFactory.GenerateProcessingAgent(
                open_ai_apiKey, AgentTask.UnitRooting, ConfigManager.TargetLanguage);

            ret.DefinitionResolver = new DefinitionResolver();
            ret.DefinitionResolver.Agent = AgentFactory.GenerateProcessingAgent(
                open_ai_apiKey, AgentTask.DefinitionResolution, ConfigManager.TargetLanguage);

            ret.DefinitionResolver.Dictionary = dictionary;

            return ret;
        }
    }
}

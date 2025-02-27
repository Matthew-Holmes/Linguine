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
    public static class StatementEngineFactory
    {

        // TODO - move as much of this stuff, especially to do with agent creation
        // to the learning extraction module

        public static StatementEngine BuildStatementEngine(API_Keys keys, ExternalDictionary dictionary)
        {
            StatementEngine ret = new StatementEngine();

            ret.ToStatementsDecomposer   = TextDecomposerFactory.MakeStatementsDecomposer(keys, ConfigManager.TargetLanguage);
            ret.FromStatementsDecomposer = TextDecomposerFactory.MakeUnitsDecomposer(keys, ConfigManager.TargetLanguage);

            // these aren't actually used any more!
            ret.ContextChangeIdentificationAgent = AgentFactory.GenerateProcessingAgent(
                keys, AgentTask.ContextChangeIdentification, ConfigManager.TargetLanguage);

            ret.ContextUpdateAgent = AgentFactory.GenerateProcessingAgent(
                keys, AgentTask.ContextUpdating, ConfigManager.TargetLanguage);

            ret.UnitRooter = new UnitRooter();
            ret.UnitRooter.Agent = AgentFactory.GenerateProcessingAgent(
                keys, AgentTask.UnitRooting, ConfigManager.TargetLanguage, true);

            // keep this on the cheaper API for now (?) since is the most expensive part
            ret.DefinitionResolver = new DefinitionResolver();
            ret.DefinitionResolver.Agent = AgentFactory.GenerateProcessingAgent(
                keys, AgentTask.DefinitionResolution, ConfigManager.TargetLanguage);

            ret.DefinitionResolver.Dictionary = dictionary; // maybe make an interface for what the dictionary is used for here

            return ret;
        }
    }
}

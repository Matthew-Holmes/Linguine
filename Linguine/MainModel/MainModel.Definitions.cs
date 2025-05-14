using Agents;
using Config;
using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine 
{ 
    public partial class MainModel
    {
        internal async Task<String> GenerateNewDefinition(DictionaryDefinition faulty)
        {
            LanguageCode target = ConfigManager.Config.Languages.TargetLanguage;

            AgentBase agent = AgentFactory.GenerateProcessingAgent(AgentTask.DefinitionRewriting, target, isHighPerformance: true);

            StringBuilder prompt = new StringBuilder();

            prompt.Append($"{PromptFactory.WordInNative(target)}: {faulty.Word}, ");
            prompt.Append($"{PromptFactory.DefinitionInNative(target)}: \n {faulty.Definition}");

            return await agent.GetResponse(prompt.ToString());
        }
    }
}

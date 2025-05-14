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

        internal async Task<ParsedDictionaryDefinition> GenerateSingleParsedDefinition(DictionaryDefinition faulty)
        {
            LearnerLevel lvl = ConfigManager.Config.GetLearnerLevel();
            LanguageCode native = ConfigManager.Config.Languages.NativeLanguage;

            HashSet<DictionaryDefinition> singleton = new HashSet<DictionaryDefinition> { faulty };

            HashSet<ParsedDictionaryDefinition> ret = await SM.Engines.DefinitionParser.ParseStatementsDefinitions(singleton, lvl, native);

            return ret.First();
        }

        internal async Task<String> GetNewIPA(DictionaryDefinition faulty)
        {
            List<DictionaryDefinition> singleton = new List<DictionaryDefinition> { faulty };

            List<Tuple<String, String>> ret = await SM.Engines.Pronouncer.GetDefinitionPronunciations(singleton);

            Tuple<String, String> ipa_romanised = ret.First();

            return ipa_romanised.Item1;

        }

        internal async Task<String> GetNewRomanised(DictionaryDefinition faulty)
        {
            List<DictionaryDefinition> singleton = new List<DictionaryDefinition> { faulty };

            List<Tuple<String, String>> ret = await SM.Engines.Pronouncer.GetDefinitionPronunciations(singleton);

            Tuple<String, String> ipa_romanised = ret.First();

            return ipa_romanised.Item2;

        }
    }
}

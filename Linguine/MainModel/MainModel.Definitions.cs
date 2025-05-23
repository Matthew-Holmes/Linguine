using Agents;
using Config;
using DataClasses;
using Helpers;
using Linguine.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine 
{
    public partial class MainModel
    {
        #region generation

        
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

        #endregion

        #region saving
        internal void UpdateRomanised(DictionaryDefinition faulty, string romanisedPronunciation, TextualEditMethod romanisedChanged)
        {
            faulty.RomanisedPronuncation = romanisedPronunciation;

            faulty.RomanisedEntryMethod = EntryMethodHelper.NewEntryMethodForTextual(
                faulty.RomanisedEntryMethod, romanisedChanged);

            using var context = _linguineDbContextFactory.CreateDbContext();

            context.Update(faulty);
            context.SaveChanges();
        }

        internal void UpdateIPA(DictionaryDefinition faulty, string ipaPronunciation, TextualEditMethod ipaChanged)
        {
            faulty.IPAPronunciation = ipaPronunciation;

            faulty.IPAEntryMethod = EntryMethodHelper.NewEntryMethodForTextual(
                faulty.IPAEntryMethod, ipaChanged);

            using var context = _linguineDbContextFactory.CreateDbContext();

            context.Update(faulty);
            context.SaveChanges();
        }

        internal void UpdateParsedDefinition(ParsedDictionaryDefinition faulty, string parsedDefinitionText, TextualEditMethod parsedDefinitionChanged)
        {
            faulty.ParsedDefinition = parsedDefinitionText;

            faulty.ParsedDefinitionEntryMethod = EntryMethodHelper.NewEntryMethodForTextual(
                faulty.ParsedDefinitionEntryMethod, parsedDefinitionChanged);

            using var context = _linguineDbContextFactory.CreateDbContext();

            context.Update(faulty);

            context.SaveChanges();
        }

        internal void UpdateCoreDefinition(DictionaryDefinition faulty, string definitionCoreText, TextualEditMethod coredDefinitionChanged)
        {
            List<String> previous = faulty.PreviousDefinitions ?? new List<string>();

            previous.Add(faulty.Definition);

            faulty.Definition = definitionCoreText;
            faulty.PreviousDefinitions = previous;

            faulty.DefinitionEntryMethod = EntryMethodHelper.NewEntryMethodForTextual(
                faulty.DefinitionEntryMethod, coredDefinitionChanged);

            using var context = _linguineDbContextFactory.CreateDbContext();

            context.Update(faulty);
            context.SaveChanges();
        }

        #endregion
    }
}

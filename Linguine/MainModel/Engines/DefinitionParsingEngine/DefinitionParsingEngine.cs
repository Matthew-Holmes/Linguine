using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Agents;


namespace Linguine
{
    internal class DefinitionParsingEngine
    {
        private ParsedDictionaryDefinitionManager PDefManager { get; set; }

        private AgentBase ParsingAgent { get; set; }

        internal DefinitionParsingEngine(ParsedDictionaryDefinitionManager pdefManager, AgentBase parsingAgent) 
        {
            PDefManager  = pdefManager;
            ParsingAgent = parsingAgent;
        }

        internal async Task ParseStatementsDefinitions(
            HashSet<DictionaryDefinition> definitions,
            LearnerLevel level,
            LanguageCode native)
        {
            HashSet<DictionaryDefinition> newDefinitionsSet = PDefManager.FilterOutKnown(definitions, level, native);
            List<DictionaryDefinition> newDefinitions = newDefinitionsSet.ToList();

            List<String> prompts = FormPrompts(newDefinitions, level);

            var getResponseTasks = prompts.Select(prompt => ParsingAgent.GetResponse(prompt));

            String[] responses = await Task.WhenAll(getResponseTasks);

            if (responses.Count() != newDefinitions.Count)
            {
                throw new Exception("failed to parse all the definitions");
            }

            HashSet<ParsedDictionaryDefinition> toAddSet = new HashSet<ParsedDictionaryDefinition>();

            for (int i = 0; i != newDefinitions.Count; i++)
            {
                ParsedDictionaryDefinition pdef = new ParsedDictionaryDefinition
                {
                    CoreDefinition = newDefinitions[i],
                    LearnerLevel = level,
                    NativeLanguage = native,
                    ParsedDefinition = responses[i]
                };
                toAddSet.Add(pdef);
            }

            PDefManager.AddSet(toAddSet);
        }


        private List<String> FormPrompts(List<DictionaryDefinition> newDefinitions, LearnerLevel level)
        {
            List<String> ret = new List<String>();

            foreach (DictionaryDefinition definition in newDefinitions) 
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("level: ");
                builder.Append(level.ToString());
                builder.Append('\n');
                builder.Append("definition: ");
                builder.Append(definition.Definition);

                ret.Add(builder.ToString());
            }

            return ret;
        }
    }
}

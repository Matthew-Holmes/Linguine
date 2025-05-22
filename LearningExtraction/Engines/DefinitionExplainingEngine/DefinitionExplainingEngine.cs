using Agents;
using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction.Engines.DefinitionExplainingEngine
{

    public interface ICanExplainDefinitions
    {
        public Task<List<DictionaryDefinitionExplanation>> ExplainDefinitions(
           List<DictionaryDefinition> definitions,
           LanguageCode native);
    }
    class DefinitionExplainingEngine : ICanExplainDefinitions
    {
        private AgentBase ExplainingAgent { get; init; }

        public DefinitionExplainingEngine(AgentBase agent)
        {
            ExplainingAgent = agent;
        }

        public async Task<List<DictionaryDefinitionExplanation>> ExplainDefinitions(
            List<DictionaryDefinition> definitions, LanguageCode native)
        {
            List<String> prompts = FormPrompts(definitions, native);

            var getResponseTasks = prompts.Select(prompt => ExplainingAgent.GetResponse(prompt));

            String[] responses = await Task.WhenAll(getResponseTasks);

            if (responses.Count() != definitions.Count)
            {
                throw new Exception("failed to explain all the definitions");
            }

            List<DictionaryDefinitionExplanation> ret = new();
            

            for (int i = 0; i != definitions.Count; i++)
            {
                DictionaryDefinitionExplanation toAdd = new DictionaryDefinitionExplanation 
                { 
                    CoreDefinition = definitions[i],
                    NativeLanguage = native,
                    Explanation = responses[i]
                };

                ret.Add(toAdd);
            }

            return ret;
        }

        private List<String> FormPrompts(List<DictionaryDefinition> definitions, LanguageCode native)
        {
            List<String> ret = new List<String>();

            // TODO - this is English specific - need to localise!

            foreach (DictionaryDefinition definition in definitions)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append('\n');
                builder.Append("word: ");
                builder.Append(definition.Word);
                builder.Append('\n');
                builder.Append("definition: ");
                builder.Append(definition.Definition);

                ret.Add(builder.ToString());
            }

            return ret;
        }
    }
}

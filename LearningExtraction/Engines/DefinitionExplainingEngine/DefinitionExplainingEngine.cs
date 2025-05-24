using Agents;
using DataClasses;
using Helpers;
using ExternalMedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.IO.Enumeration;

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
        public AgentBase DetailedAgent         { get; set; }
        public AgentBase SynonymAgent          { get; set; }
        public AgentBase DistinguishingAgent   { get; set; }
        public AgentBase RegisterAnalysisAgent { get; set; }
        public AgentBase ExampleAgent1         { get; set; }
        public AgentBase ExampleAgent2         { get; set; }
        public AgentBase ExampleAgent3         { get; set; }
        


        public async Task<List<DictionaryDefinitionExplanation>> ExplainDefinitions(
            List<DictionaryDefinition> definitions, LanguageCode native)
        {

            List<String> prompts = FormPrompts(definitions, native);

            var getDetailedTasks = prompts.Select(prompt => DetailedAgent.GetResponse(prompt));
            var getSynonymsTasks = prompts.Select(prompt => SynonymAgent.GetResponse(prompt));
            var getDisguishTasks = prompts.Select(prompt => DistinguishingAgent.GetResponse(prompt));
            var getRegisterTasks = prompts.Select(prompt => RegisterAnalysisAgent.GetResponse(prompt));
            var getExample1Tasks = prompts.Select(prompt => ExampleAgent1.GetResponse(prompt));
            var getExample2Tasks = prompts.Select(prompt => ExampleAgent2.GetResponse(prompt));
            var getExample3Tasks = prompts.Select(prompt => ExampleAgent3.GetResponse(prompt));


            await Task.WhenAll(
                getDetailedTasks.Concat(getSynonymsTasks)
                                .Concat(getDisguishTasks)
                                .Concat(getRegisterTasks)
                                .Concat(getExample1Tasks)
                                .Concat(getExample2Tasks)
                                .Concat(getExample3Tasks)
            );

            string[] detailedResults = getDetailedTasks.Select(t => t.Result).ToArray();
            string[] synonymsResults = getSynonymsTasks.Select(t => t.Result).ToArray();
            string[] disguishResults = getDisguishTasks.Select(t => t.Result).ToArray();
            string[] registerResults = getRegisterTasks.Select(t => t.Result).ToArray();
            string[] example1Results = getExample1Tasks.Select(t => t.Result).ToArray();
            string[] example2Results = getExample2Tasks.Select(t => t.Result).ToArray();
            string[] example3Results = getExample3Tasks.Select(t => t.Result).ToArray();


            List<DictionaryDefinitionExplanation> ret = new();


            for (int i = 0; i != definitions.Count; i++)
            {
                DictionaryDefinitionExplanation toAdd = new DictionaryDefinitionExplanation
                {
                    CoreDefinition             = definitions[i],
                    NativeLanguage             = native,
                    DetailedDefinition         = detailedResults[i],
                    Synonyms                   = synonymsResults[i],
                    DifferencesFromOtherSenses = disguishResults[i],
                    Register                   = registerResults[i],
                    Example1                   = example1Results[i],
                    Example2                   = example2Results[i],
                    Example3                   = example3Results[i],
                };

                ret.Add(toAdd);
            }

            return ret;
        }

        private List<String> FormPrompts(List<DictionaryDefinition> definitions, LanguageCode native)
        {
            List<String> ret = new List<String>();

            // TODO - this is English specific - need to localise!

            ForDefinitionResolution promptParts = TextFactory.DefinitionResolutionString(native);

            foreach (DictionaryDefinition definition in definitions)
            {
                StringBuilder builder = new StringBuilder();
                builder.Append('\n');
                builder.Append($"{promptParts.word}: ");
                builder.Append(definition.Word);
                builder.Append('\n');
                builder.Append($"{promptParts.singleDefinition}: ");
                builder.Append(definition.Definition);

                ret.Add(builder.ToString());
            }

            return ret;
        }
    }
}

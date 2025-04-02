using Agents;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public interface ICanPronounce
    {
        public Task<List<Tuple<String, String>>> GetDefinitionPronunciations(
            List<DictionaryDefinition> definitions);
    }

    class DefinitionPronunciationEngine : ICanPronounce
    {
        private AgentBase IPAPronunciationAgent       { get; set; }
        private AgentBase RomanisedPronunciationAgent { get; set; }

        public DefinitionPronunciationEngine(AgentBase ipa, AgentBase romanised)
        {
            IPAPronunciationAgent = ipa;
            RomanisedPronunciationAgent = romanised;
        }

        public async Task<List<Tuple<String,String>>> GetDefinitionPronunciations(
            List<DictionaryDefinition> definitions)
        {
            List<DictionaryDefinition> newDefinitions = definitions.ToList();

            List<String> prompts = FormPrompts(newDefinitions);

            var getIPAResponseTasks       = prompts.Select(prompt => IPAPronunciationAgent.GetResponse(prompt));
            var getRomanisedResponseTasks = prompts.Select(prompt => RomanisedPronunciationAgent.GetResponse(prompt));

            var IPATask       = Task.WhenAll(getIPAResponseTasks);
            var romanisedTask = Task.WhenAll(getRomanisedResponseTasks);

            String[] IPAResponses       = await IPATask;
            String[] romanisedResponses = await romanisedTask;

            if (IPAResponses.Count() != newDefinitions.Count)
            {
                throw new Exception("failed to pronounce all the definitions (IPA)");
            }

            if (romanisedResponses.Count() != newDefinitions.Count)
            {
                throw new Exception("failed to pronounce all the definitions (romanised)");
            }

            List<Tuple<String, String>> ret = new List<Tuple<String, String>>();

            for (int i = 0; i != newDefinitions.Count; i++)
            {
                ret.Add(Tuple.Create(IPAResponses[i], romanisedResponses[i]));
            }

            return ret;
        }


        private List<String> FormPrompts(List<DictionaryDefinition> newDefinitions)
        {
            List<String> ret = new List<String>();

            foreach (DictionaryDefinition definition in newDefinitions)
            {
                StringBuilder builder = new StringBuilder();
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
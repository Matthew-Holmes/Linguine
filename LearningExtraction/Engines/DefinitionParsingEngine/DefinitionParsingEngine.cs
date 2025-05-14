using System.Text;
using Agents;
using DataClasses;


namespace LearningExtraction
{
    public interface ICanParseDefinitions
    {
        public Task<HashSet<ParsedDictionaryDefinition>> ParseStatementsDefinitions(
           HashSet<DictionaryDefinition> definitions,
           LearnerLevel level,
           LanguageCode native);
    }

    public class DefinitionParsingEngine : ICanParseDefinitions
    {

        private AgentBase ParsingAgent { get; set; }

        public DefinitionParsingEngine(AgentBase parsingAgent) 
        {
            ParsingAgent = parsingAgent;
        }

        public async Task<HashSet<ParsedDictionaryDefinition>> ParseStatementsDefinitions(
            HashSet<DictionaryDefinition> definitions,
            LearnerLevel level,
            LanguageCode native)
        {
            // TODO - does it make sense to do the parsing if the learner level is native??

            List<DictionaryDefinition> newDefinitions = definitions.ToList();

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

            return toAddSet;
        }


        private List<String> FormPrompts(List<DictionaryDefinition> newDefinitions, LearnerLevel level)
        {
            List<String> ret = new List<String>();

            // TODO - this is English specific - need to localise!

            foreach (DictionaryDefinition definition in newDefinitions) 
            {
                StringBuilder builder = new StringBuilder();
                builder.Append("level: ");
                builder.Append(level.ToString());
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

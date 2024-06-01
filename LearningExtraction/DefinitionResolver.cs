using Agents;
using Infrastructure;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public class DefinitionResolver
    {
        public int LHSContextUnits { get; set; } = 5;
        public int RHSContextUnits { get; set; } = 5;

        public int MaxContextChars { get; set; } = 200;

        public AgentBase Agent { get; set; }
        public ExternalDictionary Dictionary { get; set; }


        public DefinitionResolver()
        {
        }
        
        public List<List<DictionaryDefinition>> GetPossibleDefinitions(TextDecomposition td)
        {
            return td.Units.Select(u => Dictionary.TryGetDefinition(u.Text)).ToList();
        }

        public async Task<List<int>> IdentifyCorrectDefinitions(
            List<List<DictionaryDefinition>> defs,
            TextDecomposition td,
            TextDecomposition? injective = null)
        {
            // returns the index of the correct defition
            // if the provided Decomposition td is not injective then must provide a injective version that bijects

            if (!td.Injects())
            {
                if (injective is null)
                {
                    throw new ArgumentException("non-injective decomposition used, please provide an injective one");
                }

                if (td.Units.Count != injective.Units.Count || td.Total.Text != injective.Total.Text)
                {
                    throw new ArgumentException("provided injective decomposition does not biject with provided decomposition");
                }
            }
            else
            {
                injective = td; // so we know is not null when this logic is over
            }

            // use the injective decomposition to get the context
            List<String> contexts = DecompositionHelper.GetContextWindows(
                injective, LHSContextUnits, RHSContextUnits, MaxContextChars);

            List<String> prompts = FormPromptsOneIndexed(td, defs, contexts);

            // the prompt is empty if there is no work to be done
            Task<String> defaultTask = Task<String>.Factory.StartNew(() => "0");

            var getResponseTasks = prompts.Select(prompt 
                => prompt != "" 
                    ? Agent.GetResponse(prompt) 
                    : defaultTask);

            String[] responses = await Task.WhenAll(getResponseTasks);

            List<int> correctDefnIndices = new List<int>();

            foreach (String response in responses)
            {
                int defIndex;

                try
                {

                    try
                    {
                        defIndex = int.Parse(response);
                    }
                    catch (Exception e)
                    {
                        // have another go if the agent returned the whole line, not just an integer
                        defIndex = int.Parse(response.Split('.')[0]);
                    }
                } catch (Exception e)
                {
                    throw e;
                }

                if (defIndex == -1) { defIndex++; /* keep this -1 even after reverting to zero indexing */ }

                correctDefnIndices.Add(defIndex - 1 /* used one indexing with the agent */);
            }

            return correctDefnIndices;
            
        }

        private List<String> FormPromptsOneIndexed(TextDecomposition td, List<List<DictionaryDefinition>> defs, List<String> contexts)
        {
            // gets the prompts to pass to the agent
            // if there is no decision to be made then the prompt is empty

            // TODO - need to think about how to manage multiple languages
            // this should probably live with the agent

            if (td.Units.Count != defs.Count || defs.Count != contexts.Count)
            {
                throw new ArgumentException("all provided enumerables must be the same length");
            }

            List<String> ret = new List<String>();

            for (int i = 0; i != defs.Count; i++)
            {
                // don't need to resolve anything
                if (defs[i].Count <= 1) { ret.Add(""); continue; }

                StringBuilder builder = new StringBuilder();

                builder.Append("Word: ");
                builder.Append(td.Units[i].Text);
                builder.AppendLine();

                builder.Append("Context: ");
                builder.Append(contexts[i]);
                builder.AppendLine();

                builder.Append("Definitions: ");
                for (int j = 1 /* one indexing !*/; j != defs[i].Count + 1; j++)
                {
                    builder.AppendLine();
                    builder.Append(j.ToString());
                    builder.Append(". ");
                    builder.Append(defs[i][j-1].Definition);
                }

                ret.Add(builder.ToString());
            }
            return ret;
        }
    }
}

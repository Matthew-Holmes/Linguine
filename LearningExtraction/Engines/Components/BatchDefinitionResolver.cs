using Agents;
using Infrastructure;
using System.Diagnostics;
using System.Text;
using DataClasses;
using System.Security.Cryptography;
using Serilog;
using Helpers;

namespace LearningExtraction
{
    public class BatchDefinitionResolver
    {
        public int LHSContextUnits { get; set; } = 5;
        public int RHSContextUnits { get; set; } = 5;

        public int MaxContextChars { get; set; } = 200;

        public AgentBase MultiDefinitionAgent { get; set; }
        public AgentBase SingleDefinitionAgent { get; set; }
        public DictionaryDefinitionManager DictionaryDefinitionManager { get; set; }

        public ForDefinitionResolution PromptParts { get; set; }

        public List<String> Affirmatives { get; set; }
        public List<String> Negatives { get; set; }


        public BatchDefinitionResolver()
        {
        }
        
        // TODO - refactor this to work on statements
        public List<List<DictionaryDefinition>> GetPossibleDefinitions(TextDecomposition td)
        {
            return td.Units?.Select(u => DictionaryDefinitionManager.TryGetDefinition(u)).ToList() ?? new List<List<DictionaryDefinition>>();
        }

        public async Task<List<int>> IdentifyCorrectDefinitions(
            List<List<DictionaryDefinition>> defs,
            TextDecomposition td,
            TextDecomposition? injective = null,
            List<String>? parentContext = null)
        {
            // returns the index of the correct definition
            // if the provided Decomposition td is not injective then must provide a injective version that bijects

            if (!td.Injects())
            {
                if (injective is null)
                {
                    throw new ArgumentException("non-injective decomposition used, please provide an injective one");
                }

                if (td.Units.Count != injective.Units.Count || td.Total != injective.Total)
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

            List<Prompt?> prompts = FormPromptsOneIndexed(td, defs, contexts, parentContext);

            // the prompt is empty if there is no work to be done
            Task<String> defaultTask = Task<String>.Factory.StartNew(() => "0");

            var getResponseTasks = prompts.Select(prompt 
                => prompt is not null 
                    ? 
                    (
                        prompt.isMultiDef ? MultiDefinitionAgent.GetResponse(prompt.text)
                                          : SingleDefinitionAgent.GetResponse(prompt.text)
                    ) : defaultTask);

            String[] responses = await Task.WhenAll(getResponseTasks);

            List<int> correctDefnIndices = new List<int>();

            for (int ri = 0; ri != responses.Length; ri++)
            {
                String response = responses[ri];

                if (prompts[ri] is null)
                {
                    correctDefnIndices.Add(-1);
                    continue;
                }

                if (prompts[ri].isMultiDef)
                {

                    int defIndex;

                    try
                    {
                        try
                        {
                            defIndex = int.Parse(response);
                        }
                        catch (Exception _)
                        {
                            // have another go if the agent returned the whole line, not just an integer
                            defIndex = int.Parse(response.Split('.')[0]);
                        }
                    }
                    catch (Exception _)
                    {
                        Debug.WriteLine($"failed to parse response {response}");
                        defIndex = -1;
                    }

                    if (defIndex == -1) { defIndex++; /* keep this -1 even after reverting to zero indexing */ }

                    // edge case snapping to what will become -1, i.e. no definition
                    if (defIndex - 1 > defs[ri].Count)
                    {
                        Log.Warning("agent gave an index above the total length");
                        defIndex = 0;
                    }
                    if (defIndex < -1)
                    {
                        Log.Warning("agent gave an index below -1");
                        defIndex = 0;
                    }

                    correctDefnIndices.Add(defIndex - 1 /* used one indexing with the agent */);
                } 
                else
                {
                    bool wasCorrect = false;
                    bool wasIncorrect = false;

                    foreach (String affirmative in Affirmatives)
                    {
                        if (response.Trim() == affirmative)
                        {
                            wasCorrect = true;
                        }
                    }

                    foreach (String negative in Negatives)
                    {
                        if (response.Trim() == negative)
                        {
                            Log.Information("single definition incorrect");
                            wasCorrect = true;
                        }
                    }

                    if (wasCorrect && wasIncorrect)
                    {
                        Log.Warning("result was both negative and positive, response: {response}", response);
                    }

                    if (!wasCorrect && !wasCorrect)
                    {
                        Log.Warning("couldn't parse anything from response: {response}", response);
                    }

                    correctDefnIndices.Add(wasCorrect ? 0 : -1);
                }
            }

            return correctDefnIndices;
            
        }

        private record Prompt(String text, bool isMultiDef);

        private List<Prompt?> FormPromptsOneIndexed(TextDecomposition td, List<List<DictionaryDefinition>> defs, List<String> contexts, List<String> parentContext)
        {
            // gets the prompts to pass to the agent
            // if there is no decision to be made then the prompt is empty

            // TODO - need to think about how to manage multiple languages
            // this should probably live with the agent

            if ((td.Units?.Count ?? 0) != defs.Count || defs.Count != contexts.Count)
            {
                throw new ArgumentException("all provided enumerables must be the same length");
            }

            List<Prompt?> ret = new List<Prompt?>();

            for (int i = 0; i != defs.Count; i++)
            {
                // don't need to resolve anything
                if (defs[i].Count == 0) { ret.Add(null); continue; }

                StringBuilder builder = new StringBuilder();

                builder.Append(PromptParts.word);
                builder.Append(td.Units[i]);

                builder.AppendLine();
                builder.AppendLine();

                // TODO - need to translate these if the texts is in target language

                //if (parentContext.Count > 0)
                //{
                //    builder.AppendLine("Text source summary and context:");
                //    foreach (String parentContextItem in parentContext)
                //    {
                //        builder.AppendLine(parentContextItem);
                //    }
                //    builder.AppendLine();
                //}

                if (contexts.Count > 0)
                {
                    builder.AppendLine(PromptParts.surroundingContext);
                    builder.Append(contexts[i]);
                    builder.AppendLine();
                }

                if (defs[i].Count == 1)
                {
                    builder.Append(PromptParts.singleDefinition);
                    builder.AppendLine();
                    builder.Append(defs[i][0].Definition);

                    ret.Add(new Prompt(builder.ToString(), false));
                }
                else
                {
                    builder.Append(PromptParts.definitionOptions);

                    for (int j = 1 /* one indexing !*/; j != defs[i].Count + 1; j++)
                    {
                        builder.AppendLine();
                        builder.Append(j.ToString());
                        builder.Append(". ");
                        builder.Append(defs[i][j - 1].Definition);
                    }

                    ret.Add(new Prompt(builder.ToString(), true));
                }

            }
            return ret;
        }
    }
}

using Agents;
using Infrastructure;
using LearningExtraction;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;

namespace Linguine
{
    public partial class MainModel
    {
        private TextDecomposer? ToStatementsDecomposer { get; set; }
        private TextDecomposer? FromStatementsDecomposer { get; set; }
        private UnitRooter? UnitRooter { get; set; }    
        private DefinitionResolver? DefinitionResolver { get; set; }
        private ContextChangeIdentificationAgent? ContextChangeIdentificationAgent {get; set;}
        private ContextUpdateAgent? ContextUpdateAgent { get; set;}

        public async Task<List<Statement>?> DoProcessingStep(String textualMediaName, int chars)
        {
            TextualMedia? tm = TextualMediaManager.GetByName(textualMediaName);
            if (tm is null)
            {
                return await Task.FromResult<List<Statement>?>(null);
            }

            return await DoProcessingStepInternal(tm, chars);
        }

        private async Task<List<Statement>?> DoProcessingStepInternal(TextualMedia tm, int chars)
        {
            int end = StatementManager.IndexOffEndOfLastStatement(tm);

            if (end == tm.Text.Length)
            {
                return await Task.FromResult<List<Statement>?>(null);
            }
            else if (end == 0)
            {
                throw new Exception("zero length statement implied");
            }
            else if (end == -1)
            {
                end = 0; // indicate we are at initial step, while maintaining index logic
            }

            if (tm.Text.Length - end < chars)
            {
                chars = tm.Text.Length - end;
            }

            List<String> statementTotals = await BreakIntoStatements(tm.Text.Substring(end, chars));
            List<String> previousContext;

            if (end == 0)
            {
                previousContext = await FormInitialContext(tm);
            } 
            else
            {
                Statement? previousStatement = StatementManager.GetLastStatement(tm);
                if (previousStatement is null) { throw new Exception("something went wrong"); }
                previousContext = previousStatement.StatementContext;
            }

            List<List<String>> contexts = await FormContexts(previousContext, statementTotals);

            if (contexts.Count != statementTotals.Count)
            {
                throw new Exception("something went from forming the contexts");
            }

            return await DecomposeAndAttachDefinitions(statementTotals, contexts, tm, end);
        }

        private async Task<List<Statement>> DecomposeAndAttachDefinitions(List<String> statementTotals, List<List<String>> contexts, TextualMedia parent, int startOfStatementsIndex)
        {
            if (FromStatementsDecomposer is null)
            {
                String apiKey = File.ReadLines(ConfigManager.OpenAI_APIKey).First();
                FromStatementsDecomposer = TextDecomposerFactory.MakeUnitsDecomposer(apiKey);

                UnitRooter = new UnitRooter();
                UnitRooter.Agent = new UnitRootingAgent(apiKey);

                DefinitionResolver = new DefinitionResolver();
                DefinitionResolver.Agent = new DefinitionResolutionAgent(apiKey);

                // TODO - factories?
                List<String> dictionaries = ExternalDictionaryManager.AvailableDictionaries();
                if (dictionaries.Count == 0)
                {
                    throw new Exception("no dictionary!");
                } 
                else if (dictionaries.Count > 1)
                {
                    throw new NotImplementedException();
                }
                else
                {
                    DefinitionResolver.Dictionary = ExternalDictionaryManager.GetDictionary(dictionaries[0]) 
                        ?? throw new Exception();
                }
            }
            
            var decompositionTasks = statementTotals.Select(statement =>
                FromStatementsDecomposer.DecomposeText(statement));

            TextDecomposition[] injectives = await Task.WhenAll(decompositionTasks);

            var rootingTasks = injectives.Select(
                decomp => UnitRooter?.RootUnits(decomp) ?? throw new Exception());

            TextDecomposition[] rooted = await Task.WhenAll(rootingTasks);

            List<Tuple<TextDecomposition, TextDecomposition>> decompData 
                = await AttachCorrectDefinitions(statementTotals, injectives, rooted);

            return BuildStatements(parent, startOfStatementsIndex, decompData, contexts);
        }

        private List<Statement> BuildStatements(TextualMedia parent, int startOfStatementsIndex, List<Tuple<TextDecomposition, TextDecomposition>> decompData, List<List<String>> contexts)
        {
            if (decompData.Count != contexts.Count)
            {
                throw new Exception("something went wrong");
            }

            List<Statement> ret = new List<Statement>();
            int ptr = startOfStatementsIndex;

            for(int i = 0; i != decompData.Count; i++) 
            {
                int firstCharIndex = parent.Text.IndexOf(decompData[i].Item1.Total, startOfStatementsIndex);
                int LastCharIndex = firstCharIndex + decompData[i].Item1.Total.Length - 1;

                Statement toAdd = new Statement(
                    parent,
                    firstCharIndex,
                    LastCharIndex,
                    decompData[i].Item1.Total,
                    contexts[i],
                    decompData[i].Item1,
                    decompData[i].Item2);

                ptr = LastCharIndex;

                ret.Add(toAdd);
            }

            return ret;
        }

        private async Task<List<Tuple<TextDecomposition,TextDecomposition>>> AttachCorrectDefinitions(List<string> statementTotals, TextDecomposition[] injectives, TextDecomposition[] rooted)
        {
            List<Tuple<TextDecomposition, TextDecomposition, List<List<DictionaryDefinition>>>> taskData = new List<Tuple<TextDecomposition, TextDecomposition, List<List<DictionaryDefinition>>>>();

            for(int i = 0; i < statementTotals.Count; i++)
            {
                taskData.Add(Tuple.Create(
                    injectives[i],
                    rooted[i],
                    DefinitionResolver.GetPossibleDefinitions(rooted[i])));
            }

            var correctDefTask = taskData.Select(items => DefinitionResolver.IdentifyCorrectDefinitions(
                items.Item3, items.Item2, items.Item1));

            List<int>[] correct = await Task.WhenAll(correctDefTask);  

            for (int i = 0; i != statementTotals.Count; i++)
            {
                SetCorrectDefinitions(rooted[i], correct[i], taskData[i].Item3);
            }

            List<Tuple<TextDecomposition, TextDecomposition>> ret
                = new List<Tuple<TextDecomposition, TextDecomposition>>();

            for (int i = 0; i != statementTotals.Count; i++)
            {
                // TODO - indices.
                ret.Add(Tuple.Create(injectives[i], rooted[i]));
            }

            return ret;
        }

        private void SetCorrectDefinitions(TextDecomposition textDecomposition, List<int> correctIndices, List<List<DictionaryDefinition>> possibleDefs)
        {
            if (textDecomposition.Decomposition.Count != correctIndices.Count
                      || correctIndices.Count != possibleDefs.Count)
            {
                throw new Exception("all lists must be of the same size");
            }

            for (int i = 0; i != correctIndices.Count; i++)
            {
                if (correctIndices[i] == -1)
                {
                    continue;
                }
                textDecomposition.Decomposition[i].Definition = possibleDefs[i][correctIndices[i]];
            }
        }

        private async Task<List<List<String>>> FormContexts(List<String> previousContext, List<String> statementTotals)
        {
            List<int> contextChangeStatements = await GetStatementsWhereContextChanges(previousContext, statementTotals);

            List<String> previous = previousContext;
            List<List<String>> ret = new List<List<String>>();

            for (int i = 0; i != statementTotals.Count; i++)
            {
                if (contextChangeStatements.Contains(i))
                {
                    ret.Append(await GetUpdatedContextAt(previous, statementTotals, i));
                    previous = ret.Last();
                } 
                else
                {
                    ret.Append(previous);
                }
            }

            return ret;
        }

        private async Task<List<String>> GetUpdatedContextAt(
            List<String> previous, List<String> statementTotals, int i)
        {
            if (ContextUpdateAgent is null)
            {
                String apiKey = File.ReadLines(ConfigManager.OpenAI_APIKey).First();
                ContextUpdateAgent = new ContextUpdateAgent(apiKey);
            }

            StringBuilder prompt = new StringBuilder();

            prompt.AppendLine("Old Context:");

            foreach (String contextItem in previous)
            {
                prompt.AppendLine(contextItem);
            }

            prompt.AppendLine();

            prompt.AppendLine("Changes between:");
            prompt.AppendLine(statementTotals[i - 1]);
            prompt.AppendLine(statementTotals[i]);

            prompt.AppendLine();
            prompt.Append("From Text:");

            foreach (String line in statementTotals)
            {
                prompt.AppendLine(line);
            }

            String response = await ContextUpdateAgent.GetResponse(prompt.ToString());

            return response.Split('\n').ToList();
        }

        private async Task<List<int>> GetStatementsWhereContextChanges(List<string> previousContext, List<string> statementTotals)
        {
            if (ContextChangeIdentificationAgent is null)
            {
                String apiKey = File.ReadLines(ConfigManager.OpenAI_APIKey).First();
                ContextChangeIdentificationAgent = new ContextChangeIdentificationAgent(apiKey);
            }

            StringBuilder prompt = new StringBuilder();
            prompt.AppendLine("Context: ");
            foreach (string context in previousContext)
            {
                prompt.AppendLine(context);
            }

            prompt.AppendLine("Statements: ");

            for (int i = 1; i != statementTotals.Count; i++)
            {
                prompt.Append(i.ToString());
                prompt.Append(": ");
                prompt.AppendLine(statementTotals[i]);
            }

            String response = await ContextChangeIdentificationAgent.GetResponse(prompt.ToString());

            List<String> raw = response.Split('\n').ToList();
            List<int> ret = new List<int>();

            foreach (String line in raw)
            {
                ret.Add(int.Parse(line.Split(':')[0]) - 1); // agent uses one indexing
            }

            return ret;
        }

        private async Task<List<String>> FormInitialContext(TextualMedia tm)
        {
            List<String> ret = new List<String>();

            ret.Add(tm.Description);
            ret.Add("called " + tm.Name);

            return await Task.FromResult(ret);
        }

        private async Task<List<String>> BreakIntoStatements(String chunk)
        {
            if (ToStatementsDecomposer is null)
            {
                String apiKey = File.ReadLines(ConfigManager.OpenAI_APIKey).First();
                ToStatementsDecomposer = TextDecomposerFactory.MakeStatementsDecomposer(apiKey);
            }

            TextDecomposition statementsDecomp = await ToStatementsDecomposer.DecomposeText(
                new TextualMedia { Text = chunk });

            return statementsDecomp.Units;
        }
    }
}

using Agents;
using Infrastructure;
using LearningExtraction;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Runtime.InteropServices;
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

        internal async Task ProcessNextChunk(int sessionID)
        {
            TextualMedia? tm = GetSessionFromID(sessionID)?.TextualMedia ?? null;
            if (tm is null) { return; }

            List<Statement> ret = await DoProcessingStep(tm, 1000, 20);

            if (ret is not null)
            {
                StatementManager.AddStatements(ret);
            }
        }

        private async Task<List<Statement>?> DoProcessingStep(TextualMedia tm, int chars, int maxStatements)
        {
            int end = StatementManager.IndexOffEndOfLastStatement(tm);

            if (end == tm.Text.Length)
            {
                return await Task.FromResult<List<Statement>?>(null);
            }
            else if (end == -1)
            {
                end = 0; // indicate we are at initial step, while maintaining index logic
            }

            if (tm.Text.Length - end < chars)
            {
                chars = tm.Text.Length - end;
            }

            return await DoProcessingStepWork(tm, end, chars, maxStatements);
           
        }

        private async Task<List<Statement>?> DoProcessingStepWork(
            TextualMedia tm, int end, int chars, int maxStatements)
        {
            List<StatementBuilder> builders = new List<StatementBuilder>();

            await PopulateBuilders(builders, tm, end, chars, maxStatements);

            SetIndices(builders, end);

            List<String> previousContext = await GetPreviousContext(tm);

            await FormContexts(previousContext, builders);

            await Decompose(builders);

            await AttachCorrectDefinitions(builders);

            return builders.Select(b => b.ToStatement()).ToList();
        }

        private async Task PopulateBuilders(
            List<StatementBuilder> builders, TextualMedia tm, int end, int chars, int maxStatements)
        {
            String chunk = tm.Text.Substring(end, chars);

            List<String> statementTexts = await BreakIntoStatements(chunk); // 

            if (statementTexts.Count() < 3)
            {
                throw new Exception("Failed to generate enough statements");
            }

            // no need to clip if the text is being processed all in one go
            if (tm.Text.Length != chars)
            {
                statementTexts.RemoveAt(statementTexts.Count() - 1); // remove anything that got clipped
                statementTexts.RemoveAt(statementTexts.Count() - 1); // and a bit more for good measure
            }


            foreach (String total in statementTexts)
            {
                if (String.IsNullOrWhiteSpace(total))
                {
                    continue;
                }

                builders.Add(new StatementBuilder());
                builders.Last().Parent = tm;
                builders.Last().StatementText = total;

                if (builders.Count > maxStatements) { break; }
            }
        }

        private async Task<List<String>> GetPreviousContext(TextualMedia tm)
        {
            Statement? previousStatement = StatementManager.GetLastStatement(tm);

            if (previousStatement is null)
            {
                return await FormInitialContext(tm);
            }
            else
            {
                return previousStatement.StatementContext;
            }
        }

        private async Task Decompose(List<StatementBuilder> builders)
        {
            if (FromStatementsDecomposer is null)
            {
                PrepareWorkers();
            }
            
            var decompositionTasks = builders.Select(
                b => FromStatementsDecomposer.DecomposeText(b.StatementText));

            TextDecomposition[] injectives = await Task.WhenAll(decompositionTasks);

            var rootingTasks = injectives.Select(
                decomp => UnitRooter?.RootUnits(decomp) ?? throw new Exception());

            TextDecomposition[] rooted = await Task.WhenAll(rootingTasks);

            for (int i = 0; i != builders.Count; i++)
            {
                builders[i].InjectiveDecomposition = injectives[i];
                builders[i].RootedDecomposition = rooted[i];
            }

            return;
        }

        private void PrepareWorkers()
        {
            String apiKey = File.ReadLines(ConfigManager.OpenAI_APIKey).First();

            ToStatementsDecomposer = TextDecomposerFactory.MakeStatementsDecomposer(apiKey);
            FromStatementsDecomposer = TextDecomposerFactory.MakeUnitsDecomposer(apiKey);

            ContextChangeIdentificationAgent = new ContextChangeIdentificationAgent(apiKey);
            ContextUpdateAgent = new ContextUpdateAgent(apiKey);

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

        private void SetIndices(List<StatementBuilder> builders, int startOfStatementsIndex)
        {
            if (builders.Select(b => b.Parent).Distinct().Count() != 1)
            {
                throw new Exception("something went wrong!");
            }

            String parentText = builders.FirstOrDefault().Parent.Text;

            int ptr = startOfStatementsIndex;

            foreach (StatementBuilder builder in builders) 
            {
                builder.FirstCharIndex = parentText.IndexOf(builder.StatementText, startOfStatementsIndex);
                builder.LastCharIndex  = builder.FirstCharIndex + builder.StatementText.Length - 1;
            }
        }

        private async Task AttachCorrectDefinitions(List<StatementBuilder> builders)
        {
            List<Tuple<StatementBuilder, List<List<DictionaryDefinition>>>> taskData = new List<Tuple<StatementBuilder, List<List<DictionaryDefinition>>>>();

            foreach(StatementBuilder builder in builders)
            {
                taskData.Add(Tuple.Create(
                    builder,
                    DefinitionResolver.GetPossibleDefinitions(builder.RootedDecomposition)));
            }

            var correctDefTask = taskData.Select(items => DefinitionResolver.IdentifyCorrectDefinitions(
                items.Item2, items.Item1.RootedDecomposition, items.Item1.InjectiveDecomposition, items.Item1.Context));

            List<int>[] correct = await Task.WhenAll(correctDefTask);  

            for (int i = 0; i != builders.Count; i++)
            {
                // will pass by reference and fill in the definitions
                SetCorrectDefinitions(builders[i].RootedDecomposition, correct[i], taskData[i].Item2);
            }
        }

        private void SetCorrectDefinitions(TextDecomposition textDecomposition, List<int> correctIndices, List<List<DictionaryDefinition>> possibleDefs)
        {
            if ((textDecomposition.Decomposition?.Count ?? 0) != correctIndices.Count
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

        private async Task FormContexts(List<String> previousContext, List<StatementBuilder> builders)
        {
            List<String> statementTotals = builders.Select(
                    b => b.StatementText ?? throw new Exception()).ToList();

            List<int> contextChangeStatements = await GetStatementsWhereContextChanges(
                previousContext, statementTotals);

            List<String> previous = previousContext;

            for (int i = 0; i != builders.Count; i++)
            {
                if (contextChangeStatements.Contains(i))
                {
                    builders[i].Context = await GetUpdatedContextAt(previous, statementTotals, i);
                    previous = builders[i].Context ?? throw new Exception();
                } 
                else
                {
                    builders[i].Context = previous;
                }
            }
        }

        private async Task<List<String>> GetUpdatedContextAt(
            List<String> previous, List<String> statementTotals, int i)
        {
            if (ContextUpdateAgent is null)
            {
                PrepareWorkers();
            }

            StringBuilder prompt = new StringBuilder();

            prompt.AppendLine("Old Context:");

            foreach (String contextItem in previous)
            {
                prompt.AppendLine(contextItem);
            }

            prompt.AppendLine();

            if (i == 0)
            {
                prompt.AppendLine("Needs further context at the start:");
                prompt.AppendLine(statementTotals[i]);
            } else
            {
                prompt.AppendLine("Was for following statements:");
                prompt.AppendLine();

                for (int j = 1; i - j > 0 && j < 5; j++)
                {
                    prompt.AppendLine(statementTotals[i - j]);
                }

                prompt.AppendLine();
                prompt.AppendLine("Now considering statements:");

                for (int j = 0; j < statementTotals.Count && j < 3; j++)
                {
                    prompt.Append(statementTotals[i + j]);
                }

            }

            String response = await ContextUpdateAgent.GetResponse(prompt.ToString());

            return response.Split('\n').ToList();
        }

        private async Task<List<int>> GetStatementsWhereContextChanges(
            List<string> previousContext, List<string> statementTotals)
        {
            if (ContextChangeIdentificationAgent is null)
            {
                PrepareWorkers();
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

            String promptString = prompt.ToString();

            String response = await ContextChangeIdentificationAgent.GetResponse(promptString);

            List<String> raw = response.Split('\n').ToList();
            List<int> ret = new List<int>();

            foreach (String line in raw)
            {
                if (!line.Contains(':')) { continue; }
                try
                {
                    ret.Add(int.Parse(line.Split(':')[0]) - 1); // agent uses one indexing
                } catch { continue; } // we can live without this, especially if the agent did something weird
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
                PrepareWorkers();
            }

            TextDecomposition statementsDecomp = await ToStatementsDecomposer.DecomposeText(chunk);

            return statementsDecomp.Units;
        }
    }
}

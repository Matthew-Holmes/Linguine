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
        private TextDecomposer?     ToStatementsDecomposer           { get; set; }
        private TextDecomposer?     FromStatementsDecomposer         { get; set; }
        private UnitRooter?         UnitRooter                       { get; set; }    
        private DefinitionResolver? DefinitionResolver               { get; set; }
        private AgentBase?          ContextChangeIdentificationAgent { get; set; }
        private AgentBase?          ContextUpdateAgent               { get; set; }

        internal async Task ProcessNextChunk(int sessionID)
        {
            TextualMedia? tm = GetSessionFromID(sessionID)?.TextualMedia ?? null;
            if (tm is null) { return; }

            List<Statement> ret = await DoProcessingStep(tm, 500, 20);

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

            await FindStatementsAndPopulateBuilders(builders, tm, end, chars, maxStatements);

            await FormContexts(builders);

            await DecomposeStatements(builders);

            await AttachCorrectDefinitions(builders);

            return builders.Select(b => b.ToStatement()).ToList();
        }

        private async Task FindStatementsAndPopulateBuilders(
            List<StatementBuilder> builders, TextualMedia tm, int end, int chars, int maxStatements)
        {
            String chunk = tm.Text.Substring(end, chars);

            List<String> statementTexts = await DecomposeIntoStatements(chunk); // 

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

            SetIndices(builders, end);
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
                builder.FirstCharIndex = parentText.IndexOf(builder.StatementText, ptr);
                builder.LastCharIndex  = builder.FirstCharIndex + builder.StatementText.Length - 1;

                ptr = builder.LastCharIndex + 1 ?? throw new Exception();
            }
        }

        private void PrepareWorkers()
        {
            String apiKey = File.ReadLines(ConfigManager.OpenAI_APIKey).First();

            ToStatementsDecomposer   = TextDecomposerFactory.MakeStatementsDecomposer(apiKey, ConfigManager.TargetLanguage);
            FromStatementsDecomposer = TextDecomposerFactory.MakeUnitsDecomposer(     apiKey, ConfigManager.TargetLanguage);

            ContextChangeIdentificationAgent = AgentFactory.GenerateProcessingAgent(
                apiKey, AgentTask.ContextChangeIdentification, ConfigManager.TargetLanguage);

            ContextUpdateAgent = AgentFactory.GenerateProcessingAgent(
                apiKey, AgentTask.ContextUpdating, ConfigManager.TargetLanguage);

            UnitRooter = new UnitRooter();
            UnitRooter.Agent = AgentFactory.GenerateProcessingAgent(
                apiKey, AgentTask.UnitRooting, ConfigManager.TargetLanguage);

            DefinitionResolver = new DefinitionResolver();
            DefinitionResolver.Agent = AgentFactory.GenerateProcessingAgent(
                apiKey, AgentTask.DefinitionResolution, ConfigManager.TargetLanguage);

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
    }
}

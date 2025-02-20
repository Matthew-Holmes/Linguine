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

        private ICanAnalyseText?          TextAnalyser            { get; set; }
        internal DefinitionParsingEngine? DefinitionParsingEngine { get; set; }

        private int CharsToProcess { get; set; } = 500;

        private Tuple<String?, List<String>?, bool, int> GetNextChunkInfo(TextualMedia tm)
        {
            int end = StatementManager.IndexOffEndOfLastStatement(tm);

            if (end == tm.Text.Length)
            {
                return Tuple.Create<String?, List<String>?, bool, int>(null, null, false, -1);
            }
            else if (end == -1 /* indicates no statements exist for this TextualMedia */)
            {
                end = 0; // ensure no weird stuff
            }

            int firstChar = end;

            List<String> previousContext = GetPreviousContext(tm);

            int charSpan = CharsToProcess;
            bool isTail = false;

            if (tm.Text.Length - firstChar < CharsToProcess)
            {
                charSpan = tm.Text.Length - firstChar;
                isTail = true; // TODO - should this be <=??
            }

            String chunk = tm.Text.Substring(firstChar, charSpan);

            return Tuple.Create<String?, List<String>?, bool, int>(chunk, previousContext, isTail, firstChar);
        }

        // TODO - should this return bool for success?
        internal async Task ProcessNextChunk(int sessionID)
        {
            TextualMedia? tm = GetSessionFromID(sessionID)?.TextualMedia ?? null;
            if (tm is null) { return; }

            // determine the chunk of text to process next
            (String? text, List<String>? context, bool isTail, int firstChar) = GetNextChunkInfo(tm);
            if (text is null || context is null) { return; }

            // TODO - extract interface for this - outsource to the LearningExtraction
            List<ProtoStatement>? protos = await DoProcessingStep(text, context, isTail);

            if (protos is null)
            {
                return;
            }

            List<Statement> statements = FromProtoStatements(protos, tm, firstChar);

            StatementManager.AddStatements(statements);

            if (ConfigManager.TargetLanguage != ConfigManager.NativeLanguage)
            {
                await ParseDefinitions(statements);              
            }
        }
        private List<Statement> FromProtoStatements(List<ProtoStatement> protos, TextualMedia tm, int firstChar)
        {
            List<StatementBuilder> builders = protos.Select(p => new StatementBuilder(p)).ToList();

            foreach (StatementBuilder sb in builders)
            {
                sb.Parent = tm;
            }

            SetIndices(builders, firstChar);

            List<Statement> ret = builders.Select(b => b.ToStatement()).ToList();

            return ret;
        }

        private async Task ParseDefinitions(List<Statement> statements)
        {
            if (DefinitionParsingEngine is null)
            {
                StartParsingEngine();
            }
            // TODO - what if it is null?

            HashSet<DictionaryDefinition> definitions = StatementManager.GetAllUniqueDefinitions(statements);

            await DefinitionParsingEngine.ParseStatementsDefinitions(
                definitions, ConfigManager.LearnerLevel, ConfigManager.NativeLanguage);
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
                builder.LastCharIndex = builder.FirstCharIndex + builder.StatementText.Length - 1;

                ptr = builder.LastCharIndex + 1 ?? throw new Exception();
            }
        }

        private async Task<List<ProtoStatement>?> DoProcessingStep(String text, List<String> context, bool isTail)
        {

            if (TextAnalyser is null)
            {
                StartStatementEngine();
            }

            List<ProtoStatement> protos = await TextAnalyser?.GenerateStatementsFor(text, context, isTail);

            return protos;

        }

        private void StartStatementEngine()
        {
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
                ExternalDictionary dictionary = ExternalDictionaryManager.GetDictionary(dictionaries[0])
                    ?? throw new Exception();

                API_Keys keys = ConfigManager.API_Keys;

                TextAnalyser = StatementEngineFactory.BuildStatementEngine(keys, dictionary);
            }
        }

        private void StartParsingEngine()
        {

            API_Keys keys = ConfigManager.API_Keys;

            AgentBase parsingAgent = AgentFactory.GenerateProcessingAgent(
                keys, AgentTask.DefinitionParsing, ConfigManager.NativeLanguage);

            DefinitionParsingEngine = new DefinitionParsingEngine(ParsedDictionaryDefinitionManager, parsingAgent);
        }

        private List<String> GetPreviousContext(TextualMedia tm)
        {
            Statement? previousStatement = StatementManager.GetLastStatement(tm);

            if (previousStatement is null)
            {
                return FormInitialContext(tm);
            }
            else
            {
                return previousStatement.StatementContext;
            }
        }

        // TODO - translate the word "called" here to the TargetLanguage!
        private List<String> FormInitialContext(TextualMedia tm)
        {
            List<String> ret = new List<String>();

            ret.Add(tm.Description);
            ret.Add("called " + tm.Name);

            return ret;
        }

    }
}

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

        private StatementEngine?         StatementEngine         { get; set; }
        internal DefinitionParsingEngine? DefinitionParsingEngine { get; set; }

        private int CharsToProcess { get; set; } = 500;

        private Tuple<String, List<String>>? GetNextChunk(TextualMedia tm)
        {
            int end = StatementManager.IndexOffEndOfLastStatement(tm);

            if (end == tm.Text.Length)
            {
                return null;
            }
            else if (end == -1 /* indicates no statements exist for this TextualMedia */)
            {
                end = 0; // ensure no weird stuff
            }

            int firstChar = end;

            List<String> previousContext = GetPreviousContextSerial(tm);

            int charSpan = CharsToProcess;

            if (tm.Text.Length - firstChar < CharsToProcess)
            {
                charSpan = tm.Text.Length - firstChar;
            }

            String chunk = tm.Text.Substring(firstChar, charSpan);

            return Tuple.Create(chunk, previousContext);
        }


        // TODO - should this return bool for success?
        internal async Task ProcessNextChunk(int sessionID)
        {
            TextualMedia? tm = GetSessionFromID(sessionID)?.TextualMedia ?? null;
            if (tm is null) { return; }

            // TODO - in model: determine the chunk of text to process next
            // TODO - in model: build the contextual info
            // TODO - in LearningExtraction: process text - return statements
            // TODO - in LearningExtraction: process statements - return definitions
            // TODO - in Model: once all done save the progress to disk
                // TODO - should there be checks for correctness here??

            // TODO - extract interfaces for this

            List<Statement>? ret = await DoProcessingStep(tm);

            if (ret is null)
            {
                return;
            }

            StatementManager.AddStatements(ret);

            if (ConfigManager.TargetLanguage != ConfigManager.NativeLanguage)
            {

                if (DefinitionParsingEngine is null)
                {
                    StartParsingEngine();
                }
                // TODO - what if it is null?

                HashSet<DictionaryDefinition> definitions = StatementManager.GetAllUniqueDefinitions(ret);

                await DefinitionParsingEngine.ParseStatementsDefinitions(
                    definitions, ConfigManager.LearnerLevel, ConfigManager.NativeLanguage);
            }
        }


        private async Task<List<Statement>?> DoProcessingStep(TextualMedia tm)
        {
            int end = StatementManager.IndexOffEndOfLastStatement(tm);

            if (end == tm.Text.Length)
            {
                return await Task.FromResult<List<Statement>?>(null);
            }
            else if (end == -1 /* indicates no statements exist for this TextualMedia */)
            {
                end = 0; // ensure no weird stuff
            }

            List<String> previousContext = await GetPreviousContext(tm);

            if (StatementEngine is null)
            {
                StartStatementEngine();
            }

            return await StatementEngine?.GenerateStatementsFor(tm, end, previousContext);
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

                StatementEngine = StatementEngineFactory.BuildStatementEngine(keys, dictionary);
            }
        }

        private void StartParsingEngine()
        {

            API_Keys keys = ConfigManager.API_Keys;

            AgentBase parsingAgent = AgentFactory.GenerateProcessingAgent(
                keys, AgentTask.DefinitionParsing, ConfigManager.NativeLanguage);

            DefinitionParsingEngine = new DefinitionParsingEngine(ParsedDictionaryDefinitionManager, parsingAgent);
        }

        private List<String> GetPreviousContextSerial(TextualMedia tm)
        {
            Statement? previousStatement = StatementManager.GetLastStatement(tm);

            if (previousStatement is null)
            {
                return FormInitialContextSerial(tm);
            }
            else
            {
                return previousStatement.StatementContext;
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

        // TODO - translate the word "called" here to the TargetLanguage!
        private async Task<List<String>> FormInitialContext(TextualMedia tm)
        {
            List<String> ret = new List<String>();

            ret.Add(tm.Description);
            ret.Add("called " + tm.Name);

            return await Task.FromResult(ret);
        }

        private List<String> FormInitialContextSerial(TextualMedia tm)
        {
            List<String> ret = new List<String>();

            ret.Add(tm.Description);
            ret.Add("called " + tm.Name);

            return ret;
        }

    }
}

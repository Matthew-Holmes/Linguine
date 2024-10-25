using Agents;
using Infrastructure;
using LearningExtraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.RightsManagement;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    internal partial class StatementEngine
    {
        public int CharsToProcess { get; set; } = 500;
        public int MaxStatements  { get; set; } = 20;

        internal TextDecomposer     ToStatementsDecomposer              { get; set; }
        internal TextDecomposer     FromStatementsDecomposer            { get; set; }
        internal UnitRooter         UnitRooter                          { get; set; }
        internal DefinitionResolver DefinitionResolver                  { get; set; }
        internal AgentBase          ContextChangeIdentificationAgent    { get; set; }
        internal AgentBase          ContextUpdateAgent                  { get; set; }

        internal async Task<List<Statement>?> GenerateStatementsFor(
            TextualMedia tm,
            int firstChar,
            List<String> previousContext)
        {
            List<StatementBuilder> builders = new List<StatementBuilder>();

            await FindStatementsAndPopulateBuilders(builders, tm, firstChar);

            // await FormContexts(builders, previousContext);

            await DecomposeStatements(builders);

            await AttachCorrectDefinitions(builders);

            return builders.Select(b => b.ToStatement()).ToList();
        }


        private async Task FindStatementsAndPopulateBuilders(
            List<StatementBuilder> builders, TextualMedia tm, int firstChar)
        {
            int charSpan = CharsToProcess;

            if (tm.Text.Length - firstChar < CharsToProcess)
            {
                charSpan = tm.Text.Length - firstChar;
            }

            String chunk = tm.Text.Substring(firstChar, charSpan);

            List<String> statementTexts = await DecomposeIntoStatements(chunk); // 

            if (statementTexts.Count() < 3)
            {
                throw new Exception("Failed to generate enough statements");
            }

            // no need to clip if the text is being processed all in one go
            if (tm.Text.Length != charSpan)
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

                if (builders.Count > MaxStatements) { break; }
            }

            SetIndices(builders, firstChar);
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
    }
}

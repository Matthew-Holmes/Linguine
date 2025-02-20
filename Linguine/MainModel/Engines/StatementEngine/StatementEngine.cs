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

        internal async Task<List<StatementBuilder>> GenerateStatementsFor1(
            String text, List<String> context, bool isTail)
        {
            List<StatementBuilder> builders = new List<StatementBuilder>();

            await FindStatementsAndPopulateBuilders(builders, text, context, isTail);

            return builders;
        }


        internal async Task<List<Statement>?> GenerateStatementsFor2(List<StatementBuilder> builders)
        {
            //await FormContexts(builders, previousContext);

            FormEmptyContexts(builders);

            await DecomposeStatements(builders);

            await AttachCorrectDefinitions(builders);

            return builders.Select(b => b.ToStatement()).ToList();
        }



        private async Task FindStatementsAndPopulateBuilders(
            List<StatementBuilder> builders, String text, List<String> context, bool isTail)
        {

            List<String> statementTexts = await DecomposeIntoStatements(text); 

            if (statementTexts.Count() < 3)
            {
                throw new Exception("Failed to generate enough statements");
            }

            if (isTail)
            {
                /* no need to clip if the text includes the end */
            }
            else
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
                builders.Last().StatementText = total;

                if (builders.Count > MaxStatements) { break; }
            }
        }


    }
}

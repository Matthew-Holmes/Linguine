using Agents;
using DataClasses;

namespace LearningExtraction
{
    public interface ICanAnalyseText
    {
        Task<List<ProtoStatement>?> GenerateStatementsFor(String text, List<String> context, bool isTail, CancellationToken token);
    }


    public partial class StatementEngine : ICanAnalyseText
    {
        public int MaxStatements  { get; set; } = 20;

        internal TextDecomposer     ToStatementsDecomposer              { get; set; }
        internal TextDecomposer     FromStatementsDecomposer            { get; set; }
        internal UnitRooter         UnitRooter                          { get; set; }
        internal BatchDefinitionResolver DefinitionResolver                  { get; set; }
        internal AgentBase          ContextChangeIdentificationAgent    { get; set; }
        internal AgentBase          ContextUpdateAgent                  { get; set; }

        public async Task<List<ProtoStatement>?> GenerateStatementsFor(
            String text, List<String> context, bool isTail, CancellationToken token)
        {
            List<ProtoStatementBuilder> builders = new List<ProtoStatementBuilder>();

            await FindStatementsAndPopulate(builders, text, context, isTail);

            if (token.IsCancellationRequested) { return null; }

            //await FormContexts(builders, previousContext);

            FormEmptyContexts(builders);

            await DecomposeStatements(builders);

            if (token.IsCancellationRequested) { return null; }

            await AttachCorrectDefinitions(builders);

            if (token.IsCancellationRequested) { return null; }

            return builders.Select(b => b.ToProtoStatement()).ToList();

        }


        private async Task FindStatementsAndPopulate(
            List<ProtoStatementBuilder> builders, String text, List<String> context, bool isTail)
        {

            List<String> statementTexts = await DecomposeIntoStatements(text); 

            if (statementTexts.Count() < 1)
            {
                throw new Exception("Failed to generate any statements");
            }

            if (isTail)
            {
                /* no need to clip if the text includes the end */
            }
            else
            {
                statementTexts.RemoveAt(statementTexts.Count() - 1); // remove anything that got clipped
            }

            foreach (String total in statementTexts)
            {
                if (String.IsNullOrWhiteSpace(total))
                {
                    continue;
                }

                builders.Add(new ProtoStatementBuilder());
                builders.Last().StatementText = total;

                if (builders.Count > MaxStatements) { break; }
            }
        }


    }
}

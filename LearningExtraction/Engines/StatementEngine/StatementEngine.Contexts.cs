using Infrastructure;
using LearningExtraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public partial class StatementEngine
    {
        private async Task FormContexts(List<StatementBuilder> builders, List<String> previousContext)
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

        private void FormEmptyContexts(List<ProtoStatementBuilder> builders)
        {
            for (int i = 0; i != builders.Count; i++)
            {
                builders[i].Context = new List<String>();
            }
        }

        private async Task<List<int>> GetStatementsWhereContextChanges(
         List<string> previousContext, List<string> statementTotals)
        {
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
                }
                catch { continue; } // we can live without this, especially if the agent did something weird
            }

            return ret;
        }

        private async Task<List<String>> GetUpdatedContextAt(
          List<String> previous, List<String> statementTotals, int i)
        {
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
            }
            else
            {
                prompt.AppendLine("Was for following statements:");
                prompt.AppendLine();

                for (int j = 1; i - j > 0 && j < 5; j++)
                {
                    prompt.AppendLine(statementTotals[i - j]);
                }

                prompt.AppendLine();
                prompt.AppendLine("Now considering statements:");

                for (int j = 0; j < statementTotals.Count - i && j < 3; j++)
                {
                    prompt.Append(statementTotals[i + j]);
                }

            }

            String response = await ContextUpdateAgent.GetResponse(prompt.ToString());

            return response.Split('\n').ToList();
        }
    }
}

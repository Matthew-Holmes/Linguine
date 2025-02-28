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
        private async Task<List<String>> DecomposeIntoStatements(String chunk)
        {
            TextDecomposition statementsDecomp = await ToStatementsDecomposer.DecomposeText(chunk);

            return statementsDecomp.Units;
        }

        private async Task DecomposeStatements(List<ProtoStatementBuilder> builders)
        {
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
    }
}

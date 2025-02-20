using Infrastructure;
using LearningExtraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    internal partial class StatementEngine
    {
        private async Task<List<String>> DecomposeIntoStatements(String chunk)
        {
            TextDecomposition statementsDecomp = await ToStatementsDecomposer.DecomposeText(chunk);

            return statementsDecomp.Units;
        }

        private async Task DecomposeStatements(List<ProtoStatement> protos)
        {
            var decompositionTasks = protos.Select(
                b => FromStatementsDecomposer.DecomposeText(b.StatementText));

            TextDecomposition[] injectives = await Task.WhenAll(decompositionTasks);

            var rootingTasks = injectives.Select(
                decomp => UnitRooter?.RootUnits(decomp) ?? throw new Exception());

            TextDecomposition[] rooted = await Task.WhenAll(rootingTasks);

            for (int i = 0; i != protos.Count; i++)
            {
                protos[i].InjectiveDecomposition = injectives[i];
                protos[i].RootedDecomposition = rooted[i];
            }

            return;
        }
    }
}

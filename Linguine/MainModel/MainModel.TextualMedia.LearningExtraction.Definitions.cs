using Infrastructure;
using LearningExtraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    public partial class MainModel
    {
        private async Task AttachCorrectDefinitions(List<StatementBuilder> builders)
        {
            List<Tuple<StatementBuilder, List<List<DictionaryDefinition>>>> taskData = new List<Tuple<StatementBuilder, List<List<DictionaryDefinition>>>>();

            foreach (StatementBuilder builder in builders)
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
    }
}

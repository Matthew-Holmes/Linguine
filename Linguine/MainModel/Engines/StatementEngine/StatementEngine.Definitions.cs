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
        private async Task AttachCorrectDefinitions(List<ProtoStatement> protos)
        {
            List<Tuple<ProtoStatement, List<List<DictionaryDefinition>>>> taskData = new List<Tuple<ProtoStatement, List<List<DictionaryDefinition>>>>();

            foreach (ProtoStatement proto in protos)
            {
                taskData.Add(Tuple.Create(
                    proto,
                    DefinitionResolver.GetPossibleDefinitions(proto.RootedDecomposition)));
            }

            var correctDefTask = taskData.Select(items => DefinitionResolver.IdentifyCorrectDefinitions(
                items.Item2, items.Item1.RootedDecomposition, items.Item1.InjectiveDecomposition, items.Item1.StatementContext));

            List<int>[] correct = await Task.WhenAll(correctDefTask);

            for (int i = 0; i != protos.Count; i++)
            {
                // will pass by reference and fill in the definitions
                SetCorrectDefinitions(protos[i].RootedDecomposition, correct[i], taskData[i].Item2);
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

using Infrastructure;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    public static class StatementHelper
    {
        public static Tuple<int, int>? GetStartLenOfDefinition(
            Statement statement, DictionaryDefinition def)
        {
            TextDecomposition flatInjective = statement.InjectiveDecomposition.Flattened();
            TextDecomposition flatRooted    = statement.RootedDecomposition.Flattened();

            int start = 0;
            int index = 0;

            while (index < flatInjective.Decomposition.Count)
            {
                String toFind = flatInjective.Decomposition[index].Total;
                if (statement.StatementText.Substring(start, toFind.Length) != toFind)
                {
                    start++; continue;
                }

                DictionaryDefinition? thisDef = flatRooted.Decomposition[index].Definition;

                if (thisDef is null)
                {
                    index++; continue;
                }

                if (thisDef.DatabasePrimaryKey == def.DatabasePrimaryKey)
                {
                    return Tuple.Create(start, flatInjective.Decomposition[index].Total.Length);
                }

                index++;
            }

            Log.Warning("failed to find definition {definition} in {statememt}", def.Word, statement.StatementText);
            return null;
        }
    }
}

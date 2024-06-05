using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Azure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace LearningExtraction
{
    internal static class StatementFactory
    {
        public static List<Statement> FromDatabaseEntries(
            List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>> entries)
        {
            if (entries.Count == 0)
            {
                return new List<Statement>();
            }

            Verify(entries);

            List<List<String>> contexts = BuildContexts(entries);

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            List<Statement> ret = new List<Statement>();

            for(int i = 0; i < contexts.Count; i++)
            {
                StatementDatabaseEntry entry = entries[i].Item1;

                String statement = entry.Parent.Text.Substring(
                    entry.FirstCharIndex,
                    entry.LastCharIndex - entry.FirstCharIndex + 1);

                TextDecomposition? injectiveDecompTmp = JsonConvert.DeserializeObject<TextDecomposition>(
                    entry.HeadlessInjectiveDecompositionJSON, settings);
                TextDecomposition? rootedDecompTmp   = JsonConvert.DeserializeObject<TextDecomposition>(
                    entry.HeadlessRootedDecompositionJSON,    settings);

                if (injectiveDecompTmp is null || rootedDecompTmp is null)
                {
                    throw new Exception("text decomposition deserialization failed");
                }

                // add the totals
                TextDecomposition injectiveDecomp = new TextDecomposition(statement, injectiveDecompTmp.Decomposition);
                TextDecomposition rootedDecomp    = new TextDecomposition(statement, rootedDecompTmp.Decomposition);

                AddDefinitions(rootedDecomp, entries[i].Item2);

                Statement toAdd = new Statement(entry.Parent, entry.FirstCharIndex, entry.LastCharIndex, statement, contexts[i], injectiveDecomp, rootedDecomp);

                ret.Add(toAdd);
            }

            return ret;
        }

        private static void AddDefinitions(TextDecomposition rootedDecomp, List<StatementDefinitionNode> statementDefinitionNodes)
        {
            if (statementDefinitionNodes.Count == 0) { return;}

            List<List<int>> indicesByLevel = new List<List<int>>();
            List<Queue<StatementDefinitionNode>> nodesByLevel = new List<Queue<StatementDefinitionNode>>();

            for (int i = 0; i != statementDefinitionNodes.Count; i++)
            {
                indicesByLevel.Add(new List<int>());
                nodesByLevel.Add(new Queue<StatementDefinitionNode>());

                foreach (var node in statementDefinitionNodes
                    .Where(n => n.CurrentLevel == i)
                    .OrderBy(n => n.IndexAtCurrentLevel))
                {
                    indicesByLevel[i].Add(node.IndexAtCurrentLevel);
                    nodesByLevel[i].Enqueue(node);
                }
            }

            List<int> ptrs = Enumerable.Repeat(-1, indicesByLevel.Count).ToList();
            ptrs[0] = 0;
            List<int> localPtrs = Enumerable.Repeat(0, indicesByLevel.Count).ToList();

            // use this to walk back up when we run out of definitions at a node's children
            List<TextDecomposition?> branch = Enumerable.Repeat<TextDecomposition?>(null, indicesByLevel.Count).ToList();

            branch[0] = rootedDecomp;

            // check if root has a definition (unlikely)
            int level = 0;
            if (nodesByLevel[0].Count > 0)
            {
                branch[0].Definition = nodesByLevel[0].Dequeue().DictionaryDefinition;
            }

            if (indicesByLevel.Count == 1)
            {
                return;
            }

            level++; ptrs[1] = 0;

            while (level != 0)
            {
                // check if we have definition to attach
                if (indicesByLevel[level].Contains(ptrs[level]))
                {
                    branch[level].Definition = nodesByLevel[level].Dequeue().DictionaryDefinition;
                }

                // try to move down first
                if (branch[level].Decomposition is not null && branch[level].Decomposition.Count != 0)
                {
                    branch[level + 1] = branch[level].Decomposition.First();
                    level++; ptrs[level]++; localPtrs[level] = 0;

                    continue;
                }

                // try to move right
                localPtrs[level]++;
                if (branch[level-1].Decomposition.Count < localPtrs[level])
                {
                    ptrs[level]++;
                    branch[level] = branch[level - 1].Decomposition[localPtrs[level]];
                    continue;
                }

                // can't move down, so step right at lowest possible level up the branch
                while (level != 0)
                {
                    // try to step right
                    if (localPtrs[level] < branch[level - 1].Decomposition.Count - 1)
                    {
                        localPtrs[level]++;
                        branch[level] = branch[level - 1].Decomposition[localPtrs[level]];
                        break;
                    }
                    level--; // couldn't so go up the branch
                }
            }
        }


        private static void Verify(List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>> entries)
        {
            bool listIncludesFirstStatement = entries.First().Item1.Previous is null;
            bool firstStatementHasContextCheckpoint = entries.First().Item1.ContextCheckpoint != null;

            if (!listIncludesFirstStatement || !firstStatementHasContextCheckpoint)
            {
                throw new ArgumentException("can't resolve contexts without a checkpoint!");
            }
        }

        private static List<List<String>> BuildContexts(
            List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>> entries)
        {
            List<List<String>> contexts = new List<List<String>>();

            List<String> initialContext = entries.First().Item1.ContextCheckpoint ?? new List<String>();
            contexts.Add(initialContext);

            foreach (Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>> info in entries.Skip(1))
            {
                if (!IsDescending(info.Item1.ContextDeltaRemovalsDescendingIndex))
                {
                    throw new ArgumentException("Invalid removal delta indices");
                }
                if (!IsDescending(info.Item1.ContextDeltaInsertionsDescendingIndex.Select(t => t.Item1).ToList()))
                {
                    throw new ArgumentException("Invalid insertion delta indices");
                }

                List<String> toAdd = new List<String>(contexts.Last());

                // replay the changes
                
                foreach(int x in info.Item1.ContextDeltaRemovalsDescendingIndex)
                {
                    toAdd.RemoveAt(x);
                }

                foreach(Tuple<int, String> t in info.Item1.ContextDeltaInsertionsDescendingIndex)
                {
                    toAdd.Insert(t.Item1, t.Item2);
                }

                contexts.Add(toAdd);
            }
            return contexts;
        }

        private static bool IsDescending(List<int> lst)
        {
            var testList = lst.OrderByDescending(x => x).ToList();
            for (int i = 0; i < testList.Count; i++)
            {
                if (testList[i] != lst[i])
                {
                    return false;
                }
            }
            return true;
        }

    }
}

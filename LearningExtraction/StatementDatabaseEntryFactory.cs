using Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace LearningExtraction
{
    public static class StatementDatabaseEntryFactory
    {
        public static List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>> FromStatements(List<Statement> statements, Statement? previous)
        {
            List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>> ret 
                = new List<Tuple<StatementDatabaseEntry, List<StatementDefinitionNode>>>();

            if (statements.Select(s => s.Parent).Distinct().Count() > 1)
            {
                throw new ArgumentException("multiple textual medias used!");
            }

            Statement? prev = previous;

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            foreach (Statement statement in statements)
            {
                StatementDatabaseEntry toAdd = new StatementDatabaseEntry();

                toAdd.Previous = ret.Count > 0 ? ret.Last().Item1 : null;
                toAdd.FirstCharIndex = statement.FirstCharIndex;
                toAdd.LastCharIndex = statement.LastCharIndex;
                toAdd.Parent = statement.Parent;

                if (previous is null)
                {
                    // at the start
                    toAdd.ContextCheckpoint = statement.StatementContext;
                    toAdd.ContextDeltaRemovalsDescendingIndex = new List<int>();
                    toAdd.ContextDeltaInsertionsDescendingIndex = new List<Tuple<int, string>>();
                }
                else
                {
                    toAdd.ContextDeltaRemovalsDescendingIndex = ComputeRemovalDeltas(statement, previous);
                    toAdd.ContextDeltaInsertionsDescendingIndex = ComputeInsertionDeltas(statement, previous, toAdd.ContextDeltaRemovalsDescendingIndex);
                }

                TextDecomposition headlessInjective = new TextDecomposition("", statement.InjectiveDecomposition.Decomposition);
                TextDecomposition headlessRooted    = new TextDecomposition("", statement.RootedDecomposition.Decomposition);

                toAdd.HeadlessInjectiveDecompositionJSON = JsonConvert.SerializeObject(headlessInjective, settings);
                toAdd.HeadlessRootedDecompositionJSON    = JsonConvert.SerializeObject(headlessRooted, settings);

                List<StatementDefinitionNode> defNodes = MakeDefinitionNodes(statement.RootedDecomposition, toAdd);

                ret.Add(Tuple.Create(toAdd, defNodes));

                previous = statement;
            }
            return ret;
        }

        private static List<StatementDefinitionNode> MakeDefinitionNodes(TextDecomposition headlessRooted, StatementDatabaseEntry parent)
        {
            List<StatementDefinitionNode> ret = new List<StatementDefinitionNode>();

            int level = 0;
            List<int> traversed = new List<int>();
            List<int> localPtrs = new List<int>();
            traversed.Add(0); localPtrs.Add(0);

            // use this to walk back up when we run out of definitions at a node's children
            List<TextDecomposition?> branch = new List<TextDecomposition?>();
            branch.Add(headlessRooted);

            // check if root has a definition (unlikely)
            if (headlessRooted.Definition is not null)
            {
                ret.Add(new StatementDefinitionNode
                {
                    StatementDatabaseEntry = parent,
                    DictionaryDefinition = headlessRooted.Definition,
                    CurrentLevel = 0,
                    IndexAtCurrentLevel = 0
                });
            }

            if (headlessRooted.Decomposition is null || headlessRooted.Decomposition.Count == 0) { return ret; } // edge case

            level++;
            traversed.Add(0); localPtrs.Add(0);
            branch.Add(headlessRooted.Decomposition.FirstOrDefault());
            

            while (level != 0)
            {
                // check if we have definition to attach
                if (branch[level].Definition is not null)
                {
                    ret.Add(new StatementDefinitionNode
                    {
                        StatementDatabaseEntry = parent,
                        DictionaryDefinition = branch[level].Definition,
                        CurrentLevel = level,
                        IndexAtCurrentLevel = traversed[level]
                    });
                }

                // try to move down first
                if (branch[level].Decomposition is not null && branch[level].Decomposition.Count != 0)
                {
                    if (level == branch.Count - 1)
                    {
                        // at the tip of the branch
                        branch.Add(null);
                        traversed.Add(-1);
                        localPtrs.Add(0);
                    }

                    branch[level + 1] = branch[level].Decomposition.First();
                    level++; traversed[level]++; localPtrs[level] = 0;

                    continue;
                }

                // can't move down, so step right at lowest possible level up the branch
                while (level != 0)
                {
                    // try to step right
                    if (localPtrs[level] < branch[level - 1].Decomposition.Count - 1)
                    {
                        localPtrs[level]++; traversed[level]++;
                        branch[level] = branch[level - 1].Decomposition[localPtrs[level]];
                        break;
                    }
                    level--; // couldn't so go up the branch
                }
            }

            return ret;
        }

        private static List<Tuple<int, String>> ComputeInsertionDeltas(
            Statement statement,
            Statement previous,
            List<int> removals)
        {
            List<String> tmp = new List<String>(previous.StatementContext);
            List<Tuple<int, String>> ret = new List<Tuple<int, String>>();

            foreach (int x in removals)
            {
                tmp.RemoveAt(x);
            }

            int diff = statement.StatementContext.Count - tmp.Count;
            int applied = 0;

            for (int i = statement.StatementContext.Count - 1; i >= 0; i--)
            {
                if (!previous.StatementContext.Contains(statement.StatementContext[i]))
                {
                    int pos = i - (diff - 1) + applied;
                    ret.Add(Tuple.Create(pos, statement.StatementContext[i]));
                    applied++;
                }
            }

            return ret;
        }

        private static List<int> ComputeRemovalDeltas(Statement current, Statement previous)
        {
            List<int> ret = new List<int>();
            for (int i = previous.StatementContext.Count-1; i >= 0; i--)
            {
                if (!current.StatementContext.Contains(previous.StatementContext[i]))
                {
                    ret.Add(i);
                }
            }

            return ret;
        }
    }
}

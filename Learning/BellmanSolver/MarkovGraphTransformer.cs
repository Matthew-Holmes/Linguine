using Learning.Tactics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.BellmanSolver
{
    internal static class MarkovGraphTransformer
    {
        internal static ExplodedMarkovGraph Explode(MarkovGraph markov)
        {
            List<ExplodedMarkovGraphArrow> arrows = new List<ExplodedMarkovGraphArrow>();
            Dictionary<String, double> rewards = new Dictionary<String, double>();

            // add arrows from null
            foreach (MarkovArrow oldArrow in markov.edgesFromNull)
            {
                String from = "null";
                String to = oldArrow.to.ToString().Split('.').Last();
                String middle = from + " to " + to;

                arrows.Add(new ExplodedMarkovGraphArrow(from, middle, oldArrow.prob, oldArrow.costSeconds));
                arrows.Add(new ExplodedMarkovGraphArrow(middle, to, 1.0, 0.0));

                double startOldReward = markov.rewardData.startReward;
                double endOldReward = markov.rewardData.rewards[oldArrow.to];

                double rewardDelta = endOldReward - startOldReward;

                rewards[middle] = rewardDelta;

            }

            // add the rest of the arrows
            foreach (var kvp in markov.directedEdges)
            {
                String from = kvp.Key.ToString().Split('.').Last();

                foreach (MarkovArrow oldArrow in kvp.Value)
                {
                    String to = oldArrow.to.ToString().Split('.').Last();
                    String middle = from + " to " + to;

                    arrows.Add(new ExplodedMarkovGraphArrow(from, middle, oldArrow.prob, oldArrow.costSeconds));
                    arrows.Add(new ExplodedMarkovGraphArrow(middle, to, 1.0, 0.0));

                    double startOldReward = markov.rewardData.rewards[kvp.Key];
                    double endOldReward   = markov.rewardData.rewards[oldArrow.to];

                    double rewardDelta = endOldReward - startOldReward;

                    rewards[middle] = rewardDelta;
                }
            }


            return new ExplodedMarkovGraph(arrows, rewards.AsReadOnly());
        }


        internal static MarkovGraph Prune(MarkovGraph markov)
        {
            // remove outward arrows from nodes which mathematically should not be proceeded (twisted) from
            // e.g. the node with the highest reward

            // group types by reward value (high to low)
            List<List<Type>> groupedByReward = markov.rewardData.rewards
                .GroupBy(kvp => kvp.Value)
                .OrderByDescending(g => g.Key)
                .Select(g => g.Select(kvp => kvp.Key).ToList())
                .ToList();

            if (groupedByReward.Count == 0)
            {
                return markov;
            }

            // start with all original edges
            Dictionary<Type, List<MarkovArrow>> prunedEdges = new(markov.directedEdges);

            // set of types that are considered "terminal" (no outgoing edges)
            HashSet<Type> terminalSet = new();

            for (int i = 0; i < groupedByReward.Count; i++)
            {
                List<Type> currentLevel = groupedByReward[i];

                foreach (var type in currentLevel)
                {
                    // ff this type is already a terminal, skip pruning
                    if (terminalSet.Contains(type))
                        continue;

                    bool canImprove = false;

                    // check if this node can reach any better node (in terminal set)
                    foreach (var terminal in terminalSet)
                    {
                        if (IsReachable(markov with { directedEdges = prunedEdges }, type, terminal))
                        {
                            canImprove = true;
                            break;
                        }
                    }

                    if (!canImprove)
                    {
                        // prune: remove outgoing edges from this node
                        prunedEdges.Remove(type);
                        terminalSet.Add(type);
                    }
                }
            }

            return markov with { directedEdges = prunedEdges };
        }

        private static bool IsReachable(MarkovGraph graph, Type from, Type to)
        {
            var visited = new HashSet<Type>();
            return DFS(graph, from, to, visited);
        }

        private static bool DFS(MarkovGraph graph, Type current, Type target, HashSet<Type> visited)
        {
            if (current == target)
                return true;

            if (!graph.directedEdges.TryGetValue(current, out var edges))
                return false;

            visited.Add(current);

            foreach (var edge in edges)
            {
                if (!visited.Contains(edge.to))
                {
                    if (DFS(graph, edge.to, target, visited))
                        return true;
                }
            }

            return false;
        }
    }
}

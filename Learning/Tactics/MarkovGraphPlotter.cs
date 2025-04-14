using OxyPlot;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Diagnostics;
using Config;
using Learning.BellmanSolver;
using System.Security.Cryptography.X509Certificates;

namespace Learning.Tactics
{

    internal static class MarkovGraphPlotter
    {
        internal static void SaveExplodedMarkovPlot(MarkovGraph graph, String? outFileName = null)
        {
            var dot = GenerateExplodedDot(graph);
            SavePlotFromDot(dot, outFileName);
        }

        internal static void SaveMarkovPlot(MarkovGraph graph, String? outFileName = null)
        {
            var dot = GenerateDot(graph);
            SavePlotFromDot(dot, outFileName);
        }

        private static void SavePlotFromDot(String dot, String? outFileName = null)
        {
            File.WriteAllText("graph.dot", dot);

            if (outFileName is null)
            {
                outFileName = $"plots/{ConfigManager.Config.Languages.TargetLanguage}/0000_global_mdp.png"; // so first in folder
            }
            // this requires going and installing graphviz
            // but is just for debug so thats not too big a deal
            // WARNING - this can't cope with weird characters

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "dot", // make sure it's in your PATH
                    Arguments = $"-Tpng graph.dot -o {outFileName}",
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            process.Start();
            process.WaitForExit();
        }

        private static string GetNodeName(Type t, IReadOnlyDictionary<Type, double> rewards, double avgReward)
        {
            string baseName = t.Name.Split('.').Last();

            double reward = avgReward;

            if (rewards.ContainsKey(t))
            {
                reward = rewards[t];
            }

            string rewardPart = $"\\n{reward:F2}";

            return baseName + rewardPart;
        }

        private static string GenerateExplodedDot(MarkovGraph graph)
        {
            ExplodedMarkovGraph exploded = MarkovGraphTransformer.Explode(graph);

            var sb = new StringBuilder();
            sb.AppendLine("digraph MarkovGraph {");

            foreach (ExplodedMarkovGraphArrow arrow in exploded.arrows)
            {
                sb.AppendLine($"\"{arrow.from}\"  -> \"{arrow.to}\" [label=\"p={arrow.prob:F2}\\navg={arrow.costSeconds:F1}s\"];");
            }

            sb.AppendLine("}");

            return sb.ToString();
        }

        private static string GenerateDot(MarkovGraph graph)
        {
            var sb = new StringBuilder();
            sb.AppendLine("digraph MarkovGraph {");

            foreach (var arrow in graph.edgesFromNull)
            {
                sb.AppendLine($"\"START\\n{graph.rewardData.startReward:F2}\" -> \"{GetNodeName(arrow.to, graph.rewardData.rewards, graph.avgReward)}\" [label=\"p={arrow.prob:F2}\\navg={arrow.costSeconds:F1}s\"];");
            }

            foreach (var from in graph.directedEdges)
            {
                foreach (var arrow in from.Value)
                {
                    sb.AppendLine($"\"{GetNodeName(from.Key, graph.rewardData.rewards, graph.avgReward)}\"  -> \"{GetNodeName(arrow.to, graph.rewardData.rewards, graph.avgReward)}\" [label=\"p={arrow.prob:F2}\\navg={arrow.costSeconds:F1}s\"];");
                }
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

    }
}

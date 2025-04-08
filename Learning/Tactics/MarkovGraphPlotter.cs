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

namespace Learning.Tactics
{

    public static class MarkovGraphPlotter
    {
        public static void SaveMarkovPlot(MarkovGraph graph)
        {
            var dot = GenerateDot(graph);
            File.WriteAllText("graph.dot", dot);

            string outFileName = $"plots/{ConfigManager.Config.Languages.TargetLanguage}/0000mdp.png"; // so first in folder

            // this requires going and installing graphviz
            // but is just for debug so thats not too big a deal

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

        private static string GenerateDot(MarkovGraph graph)
        {
            var sb = new StringBuilder();
            sb.AppendLine("digraph MarkovGraph {");

            foreach (var arrow in graph.edgesFromNull)
            {
                sb.AppendLine($"\"START\" -> \"{GetNodeName(arrow.to, graph.rewards, graph.avgReward)}\" [label=\"p={arrow.prob:F2}\\navg={arrow.costSeconds:F1}s\"];");
            }

            foreach (var from in graph.directedEdges)
            {
                foreach (var arrow in from.Value)
                {
                    sb.AppendLine($"\"{GetNodeName(from.Key, graph.rewards, graph.avgReward)}\"  -> \"{GetNodeName(arrow.to, graph.rewards, graph.avgReward)}\" [label=\"p={arrow.prob:F2}\\navg={arrow.costSeconds:F1}s\"];");
                }
            }

            sb.AppendLine("}");
            return sb.ToString();
        }

    }
}

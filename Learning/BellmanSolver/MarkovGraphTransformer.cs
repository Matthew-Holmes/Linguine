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
    }
}

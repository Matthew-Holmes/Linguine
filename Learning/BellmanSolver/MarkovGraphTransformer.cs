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

            // add arrows from null
            foreach (MarkovArrow oldArrow in markov.edgesFromNull)
            {
                String from = "null";
                String to = oldArrow.to.ToString().Split('.').Last();
                String middle = from + " to " + to;

                arrows.Add(new ExplodedMarkovGraphArrow(from, middle, oldArrow.prob, oldArrow.costSeconds));
                arrows.Add(new ExplodedMarkovGraphArrow(middle, to, 1.0, 0.0));
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
                }
            }

            return new ExplodedMarkovGraph(arrows);
        }
    }
}

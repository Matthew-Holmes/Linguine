using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.BellmanSolver
{

    internal record ExplodedMarkovGraphArrow(String from, String to, double prob, double costSeconds);
    internal record ExplodedMarkovGraph(List<ExplodedMarkovGraphArrow> arrows, IReadOnlyDictionary<String, double> rewards);

}

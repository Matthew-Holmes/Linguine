using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.Tactics
{
    internal record TacticTransition(Type? from, Type to, TimeSpan cost); // TODO - later we might make the model definition dependant?

    internal record MarkovArrow(Type to, double prob, double costSeconds);

    internal record MarkovGraph(
        Dictionary<Type, List<MarkovArrow>> directedEdges,
        List<MarkovArrow>                   edgesFromNull, 
        List<bool>                          edgeFromNullIsCorrect,
        double                              avgReward,
        RewardData                          rewardData,
        double                              PCorrectFirstTry);

    internal record RewardData(IReadOnlyDictionary<Type, double> rewards, double startReward);

    internal record RewardsCosts(double[] rewards, double[] costs, int cnt);
}

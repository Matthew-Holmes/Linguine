using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.Tactics
{
    public record TacticTransition(Type? from, Type to, TimeSpan cost); // TODO - later we might make the model definition dependant?

    public record MarkovArrow(Type to, double prob, double costSeconds);

    public record MarkovGraph(Dictionary<Type, List<MarkovArrow>> directedEdges, List<MarkovArrow> edgesFromNull, double avgReward, RewardData rewardData);

    public record RewardData(IReadOnlyDictionary<Type, double> rewards, double startReward);
}

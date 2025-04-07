using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;

namespace Learning.Strategy
{
    public class FeatureVectoriser
    {
        private readonly Dictionary<Type, int> _tacticTypeToIndex;

        public FeatureVectoriser(IEnumerable<FollowingSessionDatum> data, List<Type> tacticTypes)
        {
            _tacticTypeToIndex = tacticTypes.Select((type, i) => new { type, i })
                                            .ToDictionary(x => x.type, x => x.i);
        }

        public int FeatureCount => 7 + 1 + _tacticTypeToIndex.Count;

        public Vector<double> Vectorize(FollowingSessionDatum datum)
        {
            var features = new List<double>();

            var def = datum.defFeatures;
            features.Add(def.maxTimeBetweenCorrectDays);
            features.Add(def.sqrtTotalExposures);
            features.Add(def.fractionCorrect);
            features.Add(def.minTimeBetweenIncorrectDays);
            features.Add(def.avgTimeBetweenSessionsDays);
            features.Add(def.halfLifeDays);
            features.Add(1.0); // bias

            double intervalLogRatio = Math.Log(datum.intervalDays / (def.halfLifeDays + 1e-6));
            features.Add(intervalLogRatio);

            var oneHot = new double[_tacticTypeToIndex.Count];
            oneHot[_tacticTypeToIndex[datum.sessionTacticType]] = 1;
            features.AddRange(oneHot);

            return Vector<double>.Build.DenseOfEnumerable(features);
        }

        public Vector<double> GetTarget(FollowingSessionDatum datum)
        {
            return Vector<double>.Build.Dense(new[] { datum.followingWasCorrect ? 1.0 : 0.0 });
        }

        public Dictionary<Type, int> GetTacticIndexMap() => _tacticTypeToIndex;

        public List<string> GetFeatureNames()
        {
            var names = new List<string>
        {
            "maxTimeBetweenCorrectDays",
            "sqrtTotalExposures",
            "fractionCorrect",
            "minTimeBetweenIncorrectDays",
            "avgTimeBetweenSessionsDays",
            "halfLifeDays",
            "bias (intercept)",
            "log(interval / halfLife)"
        };

            // Add one-hot encoded tactic names in order of their index
            var orderedTactics = _tacticTypeToIndex.OrderBy(kv => kv.Value);
            foreach (var kvp in orderedTactics)
            {
                names.Add($"oneHot_Tactic_{kvp.Key.Name}");
            }

            return names;
        }
    }

}

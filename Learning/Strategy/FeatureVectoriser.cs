using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Learning.Strategy
{
    internal class FeatureVectoriser
    {
        private readonly Dictionary<Type, int> _tacticTypeToIndex;

        internal FeatureVectoriser(IEnumerable<FollowingSessionDatum> data, List<Type> tacticTypes)
        {
            _tacticTypeToIndex = tacticTypes.Select((type, i) => new { type, i })
                                            .ToDictionary(x => x.type, x => x.i);
        }
        //                           DiagQuad        bias
        //                         Linear  TopTriangle       
        internal int FeatureCount => 8 + 8+((8*8-8)/2) + 1 + _tacticTypeToIndex.Count;

        internal Vector<double> Vectorize(FollowingSessionDatum datum)
        {
            var features = new List<double>();

            var def = datum.defFeatures;
            double intervalLogRatio = Math.Log(datum.intervalDays / (def.halfLifeDays + 1e-6));


            var baseFeatures = new List<double>
            {
                def.maxTimeBetweenCorrectDays,
                def.sqrtTotalExposures,
                def.fractionCorrect,
                def.minTimeBetweenIncorrectDays,
                def.avgTimeBetweenSessionsDays,
                def.halfLifeDays,
                def.zipfScore,
                intervalLogRatio
            };

            features.AddRange(baseFeatures);

            for (int i = 0; i < baseFeatures.Count; i++)
            {
                for (int j = i; j < baseFeatures.Count; j++)
                {
                    features.Add(baseFeatures[i] * baseFeatures[j]);
                }
            }

            features.Add(1.0); // bias

            var oneHot = new double[_tacticTypeToIndex.Count];
            oneHot[_tacticTypeToIndex[datum.sessionTacticType]] = 1;
            features.AddRange(oneHot);

            return Vector<double>.Build.DenseOfEnumerable(features);
        }

        internal Vector<double> GetTarget(FollowingSessionDatum datum)
        {
            return Vector<double>.Build.Dense(new[] { datum.followingWasCorrect ? 1.0 : 0.0 });
        }

        internal Dictionary<Type, int> GetTacticIndexMap() => _tacticTypeToIndex;

        internal List<string> GetFeatureNames()
        {
            var names = new List<string>
            {
                "maxTimeBetweenCorrectDays",
                "sqrtTotalExposures",
                "fractionCorrect",
                "minTimeBetweenIncorrectDays",
                "avgTimeBetweenSessionsDays",
                "halfLifeDays",
                "zipfScore",
                "log(interval / halfLife)",
            };

            var productNames = new List<string>();

            for (int i = 0; i < names.Count; i++)
            {
                for (int j = i; j < names.Count; j++)
                {
                    productNames.Add(names[i] + " x " + names[j]);
                }
            }


            names.AddRange(productNames);

            names.Add("bias");


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

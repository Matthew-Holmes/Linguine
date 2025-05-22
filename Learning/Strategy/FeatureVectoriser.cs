using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Serilog;

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
        //                             DiagQuad              bias
        //                         Linear       TopTriangle                          one hot and 3x decays
        internal int FeatureCount => 10 + 10 +((10*10-10)/2) + 1 + _tacticTypeToIndex.Count * 4;

        internal Vector<double> Vectorize(FollowingSessionDatum datum)
        {
            var features = new List<double>();


            // different rates since coefficients will go into the log as powers
            var def = datum.defFeatures;
            double intervalLogRatio1 = Math.Log(1.0 * datum.intervalDays / (def.halfLifeDays + 1e-6));
            double intervalLogRatio2 = Math.Log(2.0 * datum.intervalDays / (def.halfLifeDays + 1e-6));
            double intervalLogRatio3 = Math.Log(0.5 * datum.intervalDays / (def.halfLifeDays + 1e-6));


            var baseFeatures = new List<double>
            {
                def.maxTimeBetweenCorrectDays,
                def.sqrtTotalExposures,
                def.fractionCorrect,
                def.minTimeBetweenIncorrectDays,
                def.avgTimeBetweenSessionsDays,
                def.halfLifeDays,
                def.zipfScore,
                intervalLogRatio1,
                intervalLogRatio2,
                intervalLogRatio3,
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
            if (_tacticTypeToIndex.ContainsKey(datum.sessionTacticType))
            {
                oneHot[_tacticTypeToIndex[datum.sessionTacticType]] = 1;
            } else
            {
                Log.Error("couldn't find tactic in tactic dictionary!");
            }
            features.AddRange(oneHot);

            features.AddRange(oneHot.Select(x => x * intervalLogRatio1).ToArray());
            features.AddRange(oneHot.Select(x => x * intervalLogRatio2).ToArray());
            features.AddRange(oneHot.Select(x => x * intervalLogRatio3).ToArray());

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
                "log(2.0 * interval / halfLife)",
                "log(0.5 * interval / halfLife)",
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

            // TODO - add the single features each per tactic as extra predictors
            // Add one-hot encoded tactic names in order of their index
            var orderedTactics = _tacticTypeToIndex.OrderBy(kv => kv.Value);
            foreach (var kvp in orderedTactics)
            {
                names.Add($"oneHot_Tactic_{kvp.Key.Name}");
            }

            foreach (var kvp in orderedTactics)
            {
                names.Add($"oneHot_Tactic_{kvp.Key.Name}_timedecayed_1.0");
            }
            foreach (var kvp in orderedTactics)
            {
                names.Add($"oneHot_Tactic_{kvp.Key.Name}_timedecayed_2.0");
            }
            foreach (var kvp in orderedTactics)
            {
                names.Add($"oneHot_Tactic_{kvp.Key.Name}_timedecayed_0.5");
            }

            return names;
        }
    }

}

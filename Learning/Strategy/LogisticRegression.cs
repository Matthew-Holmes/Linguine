using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.Strategy
{

    public class LogisticRegression
    {
        private readonly Vector<double> _parameters;

        public LogisticRegression(List<FollowingSessionDatum> data, List<Type> tacticsUsed)
        {
            var vectoriser = new FeatureVectoriser(data, tacticsUsed);

            var X = Matrix<double>.Build.Dense(data.Count, vectoriser.FeatureCount);
            var y = Vector<double>.Build.Dense(data.Count);

            for (int i = 0; i < data.Count; i++)
            {
                X.SetRow(i, vectoriser.Vectorize(data[i]));
                y[i] = data[i].followingWasCorrect ? 1.0 : 0.0;
            }

            var initialParams = Vector<double>.Build.Dense(vectoriser.FeatureCount, 0.0);

            // objective function
            var objective = new Func<Vector<double>, (double, Vector<double>)>(p =>
            {
                var z = X * p;

                var predictions = z.Map(v =>
                {
                    double pred = 1.0 / (1.0 + Math.Exp(-v));
                    double epsilon = 1e-8;
                    return Math.Min(Math.Max(pred, epsilon), 1 - epsilon);
                });
                // first part of loss: y * log(p)
                var correctPart = y.PointwiseMultiply(predictions.Map(Math.Log));

                // second part: (1 - y) * log(1 - p)
                var incorrectPart = y.Map(v => 1 - v)
                                     .PointwiseMultiply(predictions.Map(p => Math.Log(1 - p)));

                // total loss
                double loss = -(correctPart.Sum() + incorrectPart.Sum());

                var gradient = X.TransposeThisAndMultiply(predictions - y);

                Log.Information("registered CE loss of {loss} per datum", loss / data.Count);

                return (loss, gradient);
            });

            var objFunc = ObjectiveFunction.Gradient(objective);
            
            var minimizer = new BfgsMinimizer(gradientTolerance: 1e-5, parameterTolerance: 1e-5, functionProgressTolerance: 1e-5, maximumIterations: 1000);

            var result = minimizer.FindMinimum(objFunc, initialParams);
            _parameters = result.MinimizingPoint;

            LogModelParameters(_parameters, vectoriser);

        }

        public double PredictProbability(FollowingSessionDatum datum, List<Type> types)
        {
            var vectorizer = new FeatureVectoriser(new List<FollowingSessionDatum> { datum }, types);
            var features = vectorizer.Vectorize(datum);
            var z = _parameters * features;
            
            
            return 1.0 / (1.0 + Math.Exp(-z));
        }


        public static void LogModelParameters(Vector<double> parameters, FeatureVectoriser vectoriser)
        {
            var featureNames = vectoriser.GetFeatureNames();

            Log.Information("📊 Fitted Model Parameters:");
            for (int i = 0; i < parameters.Count; i++)
            {
                Log.Information("  {Index,2}: {Name,-40} = {Value,8:F4}", i, featureNames[i], parameters[i]);
            }
        }

    }
}

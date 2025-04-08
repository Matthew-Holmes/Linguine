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
    public class FeatureScaler
    {
        private Vector<double> _mins;
        private Vector<double> _maxs;

        public Vector<double> Mins => _mins;
        public Vector<double> Maxs => _maxs;
            

        public void Fit(Matrix<double> X)
        {
            int cols = X.ColumnCount;
            _mins = Vector<double>.Build.Dense(cols, double.PositiveInfinity);
            _maxs = Vector<double>.Build.Dense(cols, double.NegativeInfinity);

            for (int i = 0; i < X.RowCount; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    double val = X[i, j];
                    if (val < _mins[j]) _mins[j] = val;
                    if (val > _maxs[j]) _maxs[j] = val;
                }
            }
        }

        public void Transform(Matrix<double> X)
        {
            for (int i = 0; i < X.RowCount; i++)
            {
                for (int j = 0; j < X.ColumnCount; j++)
                {
                    double range = _maxs[j] - _mins[j];
                    if (range == 1)
                        continue; // likely a one-hot

                    if (range != 0)
                        X[i, j] = 2 * (X[i, j] - _mins[j]) / range - 1;
                    else
                        X[i, j] = 1.0; // bias is constant
                }
            }
        }

        public Vector<double> Transform(Vector<double> x)
        {
            var scaled = Vector<double>.Build.Dense(x.Count);
            for (int j = 0; j < x.Count; j++)
            {
                double range = _maxs[j] - _mins[j];
                if (range != 0)
                    scaled[j] = 2 * (x[j] - _mins[j]) / range - 1;
                else
                    scaled[j] = 0;
            }
            return scaled;
        }
    }



    public class LogisticRegression
    {
        private readonly Vector<double> _parameters;
        private double Normaliser = 1e-3;

        private FeatureScaler _scaler;

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

            _scaler = new FeatureScaler();
            _scaler.Fit(X);
            _scaler.Transform(X);

            var initialParams = Vector<double>.Build.Dense(vectoriser.FeatureCount, 0.0);

            // objective function
            var objective = new Func<Vector<double>, (double, Vector<double>)>(p =>
            {
                var z = X * p;

                var predictions = z.Map(v =>
                {
                    double pred = 1.0 / (1.0 + Math.Exp(-v));
                    double epsilon = 1e-12;
                    return Math.Min(Math.Max(pred, epsilon), 1 - epsilon);
                });
                // first part of loss: y * log(p)
                var correctPart = y.PointwiseMultiply(predictions.Map(Math.Log));

                // second part: (1 - y) * log(1 - p)
                var incorrectPart = y.Map(v => 1 - v)
                                     .PointwiseMultiply(predictions.Map(p => Math.Log(1 - p)));

                // total loss
                double loss = -(correctPart.Sum() + incorrectPart.Sum()) + p.DotProduct(p) * Normaliser;
                var regularization = p * (2.0 * Normaliser);

                var gradient = X.TransposeThisAndMultiply(predictions - y);

                Log.Information("registered CE loss of {loss} per datum", loss / data.Count);

                return (loss, gradient + regularization);
            });

            var objFunc = ObjectiveFunction.Gradient(objective);
            
            var minimizer = new BfgsMinimizer(gradientTolerance: 1e-5, parameterTolerance: 1e-5, functionProgressTolerance: 1e-5, maximumIterations: 1000);

            var result = minimizer.FindMinimum(objFunc, initialParams);
            _parameters = result.MinimizingPoint;

            LogModelParameters(_parameters, vectoriser, _scaler);

        }

        public double PredictProbability(FollowingSessionDatum datum, List<Type> types)
        {
            var vectorizer = new FeatureVectoriser(new List<FollowingSessionDatum> { datum }, types);
            var features = vectorizer.Vectorize(datum);
            var scaledFeatures = _scaler.Transform(features); 
            var z = _parameters * scaledFeatures;

            return 1.0 / (1.0 + Math.Exp(-z));
        }


        public static void LogModelParameters(Vector<double> parameters, FeatureVectoriser vectoriser, FeatureScaler scaler)
        {
            var featureNames = vectoriser.GetFeatureNames();

            Log.Information("📊 Fitted Model Parameters (unscaled):");

            for (int i = 0; i < parameters.Count; i++)
            {
                double min = scaler.Mins[i];
                double max = scaler.Maxs[i];
                double range = max - min;

                if (range == 0)
                {
                    Log.Information("  {Index,2}: {Name,-40} = {Value,8:F4} (⚠️ constant feature)", i, featureNames[i], 1.0);
                    continue;
                }

                // Adjust the parameter as if features were not scaled
                double adjustedParam = parameters[i] * 2 / range;
                double biasEffect = -adjustedParam * min;

                Log.Information("  {Index,2}: {Name,-40} = {Value,8:F4} (adjusted), range = [{Min:F4}, {Max:F4}], biasEffect = {Bias:+0.####;-0.####;0}",
                    i, featureNames[i], adjustedParam, min, max, biasEffect);
            }
        }


    }
}

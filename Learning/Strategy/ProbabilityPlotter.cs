using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using OxyPlot.SkiaSharp;
using DataClasses;
using OxyPlot.Legends;

namespace Learning.Strategy
{

    public static class ProbabilityPlotter
    {
        public static void PlotProbabilityCurves(
            LogisticRegression model,
            DefinitionFeatures defFeatures,
            IEnumerable<LearningTactic> tactics,
            string outputPath)
        {
            var plotModel = new PlotModel { Title = $"Prediction Curves for '{defFeatures.def.Word}'" };
            var legend = new Legend
            {
                LegendTitle = "Learning Tactics",
                LegendPosition = LegendPosition.RightTop,
                LegendPlacement = LegendPlacement.Outside,
                LegendOrientation = LegendOrientation.Vertical,
                LegendBorderThickness = 0
            };
            plotModel.Legends.Add(legend);

            double lookAheadDays = 200;

            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Bottom,
                Title = "Interval Days",
                Minimum = 0,
                Maximum = lookAheadDays
            });

            plotModel.Axes.Add(new LinearAxis
            {
                Position = AxisPosition.Left,
                Title = "Probability Correct",
                Minimum = 0,
                Maximum = 1
            });

            var intervalRange = GenerateRange(0.1, lookAheadDays, 400); // 

            List<Type> types = tactics.Select(t => t.GetType()).ToList();

            foreach (var tactic in tactics)
            {
                var tacticType = tactic.GetType();
                string keyLabel = tacticType.ToString().Split('.').Last(); // Extract just the final part of the type name


                var series = new LineSeries
                {
                    Title = keyLabel,  
                    StrokeThickness = 2,
                    MarkerType = MarkerType.None
                };

                foreach (var intervalDays in intervalRange)
                {
                    var datum = CreateDatum(defFeatures, tactic, intervalDays);
                    double prob = model.PredictProbability(datum, types);
                    series.Points.Add(new DataPoint(intervalDays, prob));
                }

                plotModel.Series.Add(series);
            }

            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }


            using var stream = File.Create(outputPath);
            var exporter = new PngExporter { Width = 800, Height = 600 };
            exporter.Export(plotModel, stream);
        }

        private static List<double> GenerateRange(double start, double end, int count)
        {
            var range = new List<double>();
            double step = (end - start) / (count - 1);
            for (int i = 0; i < count; i++)
                range.Add(start + i * step);
            return range;
        }

        private static FollowingSessionDatum CreateDatum(DefinitionFeatures features, LearningTactic tactic, double interval)
        {
            return new FollowingSessionDatum(
                defFeatures: features,
                session: tactic,
                intervalDays: interval,
                followingWasCorrect: false // prediction ignores this
            );
        }
    }

}

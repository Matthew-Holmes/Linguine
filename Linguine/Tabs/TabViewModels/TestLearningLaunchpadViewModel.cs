﻿using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System.Linq;
using System;
using System.Windows.Input;
using UserInputInterfaces;
using Learning;

namespace Linguine.Tabs 
{ 
    public class TestLearningLaunchpadViewModel : TabViewModelBase
    {
        public ICommand FreeStudyCommand            { get; private set; }
        public ICommand TargetedStudyCommand        { get; private set; }
        public ICommand StartVocabAssessmentCommand { get; private set; }





        public TestLearningLaunchpadViewModel(UIComponents uiComponents, MainModel model, MainViewModel parent) : base(uiComponents, model, parent)
        {
            Title = "Learn";

            FreeStudyCommand            = new RelayCommand(() => BeginFreeStudy());
            TargetedStudyCommand        = new RelayCommand(() => BeginTargetedStudy());
            StartVocabAssessmentCommand = new RelayCommand(() => BeginVocabAssessment());

            ValidateSufficentData();

            if (AnyDataForWordFrequencies)
            {
                toGraph = _model.GetPKnownByBinnedZipf();
                CreatePlot();
            }
        }

        #region data validation

        public bool EnoughDataForWordFrequencies { get; private set; }
        public bool AnyDataForWordFrequencies { get; private set; }
        public bool NeedToBurnInVocabularyData { get; private set; }

        public bool NeedADictionary { get; private set; }

        public bool TellUserToProcessMoreData => !EnoughDataForWordFrequencies && !NeedADictionary;
        public bool TellUserToDoVocabBurnin => NeedToBurnInVocabularyData && EnoughDataForWordFrequencies && !NeedADictionary;
        public bool FreeStudyIsEnabled => EnoughDataForWordFrequencies && !NeedToBurnInVocabularyData;
        public bool TargetedStudyEnabled => AnyDataForWordFrequencies;

        public String NeedADictionaryText { get; } = "Please import a dictionary to begin learning";
        public String NeedMoreDataText { get; } = "Not enough processed text for learning";
        public String NeedVocabBurnInText { get; } = "Please complete an initial vocabulary assessment";

        private void ValidateSufficentData()
        {
            NeedADictionary              = _model.NeedToImportADictionary;
            EnoughDataForWordFrequencies = _model.EnoughDataForWordFrequencies();
            AnyDataForWordFrequencies    = _model.AnyDataForWordFrequencies();
            NeedToBurnInVocabularyData   = _model.NeedToBurnInVocabularyData();
        }

        private void BeginTargetedStudy()
        {
            _parent.CloseThisAndBeginTargetedStudy(this);
        }

        private void BeginFreeStudy()
        {
            _parent.CloseThisAndBeginFreeStudy(this);
        }

        private void BeginVocabAssessment()
        {
            _parent.CloseThisAndBeginVocabAssessment(this);
        }

        #endregion


        #region plots 
        private bool _showPlot = false;

        public bool ShowPlot
        {
            get => _showPlot;
            set
            {
                _showPlot = value;
                OnPropertyChanged(nameof(_showPlot));
            }
        }

        private Tuple<double[], double[]> toGraph;

        private PlotModel _plotModel;

        public PlotModel PlotModel
        {
            get => _plotModel;
            private set
            {
                _plotModel = value;
                OnPropertyChanged(nameof(PlotModel));
            }
        }

        private void CreatePlot()
        {
            var (xs, ys) = toGraph;

            var model = new PlotModel { Title = "Zipf Distribution" };

            model.PlotAreaBorderThickness = new OxyThickness(left: 1, top: 0, right: 0, bottom: 1);

            var lineSeries = new LineSeries
            {
                Title = "P(Known)",
                MarkerType = MarkerType.Circle,
                LineStyle = LineStyle.Solid
            };

            for (int i = 0; i < xs.Length; i++)
            {
                lineSeries.Points.Add(new DataPoint(xs[i], ys[i]));
            }
            model.Series.Add(lineSeries);

            var sortedXs = xs.Distinct().OrderBy(v => v).ToList();

            double xMargin = (sortedXs.Last() - sortedXs.First()) * 0.05;

            var xAxis = new LinearAxis
            {
                Position = AxisPosition.Bottom,
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                TickStyle = TickStyle.Inside,
                AxislineStyle = LineStyle.Solid,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MinorTickSize = 0,
                Minimum = sortedXs.First() - xMargin,
                Maximum = sortedXs.Last() + xMargin
            };

            var yAxis = new LinearAxis
            {
                Position = AxisPosition.Left,
                MajorGridlineStyle = LineStyle.None,
                MinorGridlineStyle = LineStyle.None,
                AxislineStyle = LineStyle.Solid,
                IsPanEnabled = false,
                IsZoomEnabled = false,
                MinorTickSize = 0,
                TickStyle = TickStyle.Inside,
                Minimum = 0,
                Maximum = 1.05
            };


            model.Axes.Add(xAxis);
            model.Axes.Add(yAxis);

            PlotModel = model;
            ShowPlot = true;
        }
        #endregion
    }
}

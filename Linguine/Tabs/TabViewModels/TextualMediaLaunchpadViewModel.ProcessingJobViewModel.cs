using Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Linguine.Tabs
{
    internal partial class TextualMediaLaunchpadViewModel
    {

        internal class ProcessingJobViewModel : ViewModelBase
        {
            private int       _total;
            private int       _complete;
            private decimal   _secondsPerStep;
            private int       _charPerStep;
            private decimal   _stopAtPercentage      = 100m;
            private bool      _isProcessing;
            private string    _textName;
            private MainModel _model;

            public ProcessingJobViewModel(MainModel model, 
                                          string    textName, 
                                          int       total,
                                          int       complete,
                                          decimal   secPerStep,
                                          int       charPerStep,
                                          bool      isProcessing)
            {
                _model          = model;
                _textName       = textName;
                _total          = total;
                _complete       = complete;
                _secondsPerStep = secPerStep;
                _charPerStep    = charPerStep;
                _isProcessing   = isProcessing;

                ToggleProcessingCommand = new RelayCommand(ToggleProcessing);
                _runningProcessedPercentage = PercentageHelper.TunePrecision(
                    100m * ((decimal)_complete / (decimal)_total));
            }

            public ProcessingJobViewModel(MainModel model, MainModel.ProcessingJobInfo info)
                : this(model, info.TextName, info.Total, info.Complete, info.SecondsPerStep, info.CharPerStep, info.IsProcessing)
            {
                
            }

            public string TextName { get => _textName; }


            private decimal _runningProcessedPercentage;
            public decimal RunningProcessedPercentage
            {
                get => _runningProcessedPercentage;
                set
                {
                    _runningProcessedPercentage = value;
                    OnPropertyChanged(nameof(RunningProcessedPercentage));
                }
            }

            public decimal StopAtPercentage
            {
                get => _stopAtPercentage;
                set { _stopAtPercentage = value; OnPropertyChanged(nameof(StopAtPercentage)); }
            }

            private CancellationTokenSource _cts;
            private int _smoothingMillisec = 666;


            // This got ugly, couldn't definitely be made less crazy...
            // but it does work
            private async Task DoPercentageSmoothing()
            {
                int step = _steps;
                int lastComplete = _complete;
                int penultimate  = _complete;

                decimal multiplier = 1.1m;
                decimal smooth = 100.0m * (decimal)(_complete) / (decimal)(_total);
                decimal newSmooth = smooth;

                decimal overrunDelta = 0.0m;

                // we'll keep a rolling average of how long the steps take
                Queue<Tuple<int, decimal>> previous = new Queue<Tuple<int, decimal>>(capacity: 3);
                previous.Enqueue(Tuple.Create(_charPerStep, _secondsPerStep));


                Stopwatch stopWatch = new Stopwatch();
                stopWatch.Start();

                while (!_cts.IsCancellationRequested)
                {

                    await Task.Delay(_smoothingMillisec);

                    if (step != _steps)
                    {
                        // book-keeping
                        previous.Enqueue(
                                Tuple.Create(_complete - lastComplete,
                                             stopWatch.ElapsedMilliseconds / 1_000m));
                        // advance our history
                        step = _steps;
                        penultimate  = lastComplete;
                        lastComplete = _complete;
                    }

                    if (_complete == penultimate) { continue; /* no updates yet */ }

                    long elapse_ms = stopWatch.ElapsedMilliseconds;

                    stopWatch.Restart();

                    decimal averageCharPerStep = (decimal)previous.Select(t => t.Item1).Average();
                    decimal averageSecPerStep  = (decimal)previous.Select(t => t.Item2).Average();

                    decimal projectedCompletedCharsDelta = (elapse_ms / (averageSecPerStep * 1_000)) * averageCharPerStep;
                    decimal projectedPercentageDelta = 100.0m * (projectedCompletedCharsDelta / _total);

                    decimal truePercentage = 100.0m * (decimal)_complete / (decimal)_total;
                    decimal maxDelta       = 100.0m * (decimal)(_complete - penultimate) / (decimal)_total;

                    decimal lastSmooth;

                    if (smooth < truePercentage)
                    {
                        // increase the data because our progress has fallen behind the penultimate
                        // "true" completed percentage
                        multiplier *= 1.1m;
                        overrunDelta = 0.0m;
                        lastSmooth = smooth;
                        newSmooth = smooth + multiplier * projectedPercentageDelta;
                    } else if (smooth < truePercentage + 0.9m * maxDelta)
                    {
                        // standard progress
                        multiplier = 1.0m;
                        overrunDelta = 0.0m;
                        lastSmooth = smooth;
                        newSmooth = smooth + projectedPercentageDelta;
                    }
                    else
                    {
                        multiplier = 1.0m;
                        overrunDelta += projectedPercentageDelta;

                        // we are getting closer to the largest genuine progress percentage
                        // tweak the smooth value so that it asymptotically approaches it
                        double x = 10.0 * (double)(
                            (smooth + overrunDelta)
                          - (truePercentage + 0.9m * maxDelta)) / (double)(maxDelta);
                        double y = Math.Exp(x) / (1.0 + Math.Exp(x)); // sigmoid y < 1.0
                        // TODO - can we get the d/dx at zero to be 1.0 here to get smooth speed?

                        lastSmooth = newSmooth;
                        newSmooth = truePercentage + 0.9m * maxDelta + 0.1m * (decimal)y * maxDelta; // < true + max
                    }

                    // use our method for ensuring percentages are distinct so we always see motion
                    // TODO - something custom here
                    List<decimal> tidied = PercentageHelper.RoundDistinctPercentages(new List<decimal>
                    {
                        lastSmooth, newSmooth
                    });

                    RunningProcessedPercentage = tidied.Last();

                    if (overrunDelta == 0.0m)
                    { 
                        smooth = newSmooth;
                    }
                    else
                    {
                        // in an edge case
                    }
                }
            }

            public bool IsProcessing
            {
                get => _isProcessing;
                private set 
                { 
                    _isProcessing = value;
                    OnPropertyChanged(nameof(IsProcessing));
                    OnPropertyChanged(nameof(ButtonText));

                    if (value)
                    {
                        _cts = new CancellationTokenSource();
                        Task.Run(() => DoPercentageSmoothing());
                    }
                    else
                    {
                        _cts.Cancel();
                        RunningProcessedPercentage = PercentageHelper.TunePrecision(
                            100.0m * (decimal)_complete / (decimal)_total);
                    }
                }
            }

            public string ButtonText => IsProcessing ? "Stop" : "Start";

            public ICommand ToggleProcessingCommand { get; }

            private int _steps = 0;
            private async void ToggleProcessing()
            {
                IsProcessing = !IsProcessing;

                if (IsProcessing)
                {
                    await Task.Run(() => _model.StartBulkProcessing(
                        _textName,
                        complete =>
                        {
                            _complete = complete;
                            _steps++;
                        }));
                }
                else
                {
                    _model.StopBulkProcessing(_textName); // TODO - do this on dispose
                    _steps = 0;
                }
            }
        }

    }
}

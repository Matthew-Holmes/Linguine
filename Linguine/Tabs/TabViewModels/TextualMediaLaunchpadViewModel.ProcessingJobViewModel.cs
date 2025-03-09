using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Linguine.Tabs
{
    internal partial class TextualMediaLaunchpadViewModel
    {

        internal class ProcessingJobViewModel : ViewModelBase
        {
            private decimal   _processedPercentage;
            private decimal   _stopAtPercentage      = 100m;
            private bool      _isProcessing;
            private string    _textName;
            private MainModel _model;

            public ProcessingJobViewModel(MainModel model, 
                                          string    textName, 
                                          decimal   currentPct, 
                                          bool      isProcessing)
            {
                _model               = model;
                _textName            = textName;
                _processedPercentage = currentPct;
                _isProcessing        = isProcessing;

                ToggleProcessingCommand = new RelayCommand(ToggleProcessing);
            }

            public ProcessingJobViewModel(MainModel model, MainModel.ProcessingJobInfo info)
                : this(model, info.TextName, info.CurrentPct, info.IsProcessing)
            {
                
            }

            public string TextName { get => _textName; }

            public decimal ProcessedPercentage
            {
                get => PercentageHelper.TunePrecision(_processedPercentage);
                set { _processedPercentage = value; OnPropertyChanged(); }
            }

            public decimal StopAtPercentage
            {
                get => _stopAtPercentage;
                set { _stopAtPercentage = value; OnPropertyChanged(); }
            }

            public bool IsProcessing
            {
                get => _isProcessing;
                private set { _isProcessing = value; OnPropertyChanged(); OnPropertyChanged(nameof(ButtonText)); }
            }

            public string ButtonText => IsProcessing ? "Stop" : "Start";

            public ICommand ToggleProcessingCommand { get; }


            private async void ToggleProcessing()
            {
                IsProcessing = !IsProcessing;

                if (IsProcessing)
                {
                    await Task.Run(() => _model.StartBulkProcessing(
                        _textName,
                        progress =>
                        { 
                            ProcessedPercentage = progress;
                        }));
                }
                else
                {
                    _model.StopBulkProcessing(_textName); // TODO - do this on dispose
                }
            }
        }

    }
}

using Agents;
using Agents.DummyAgents;
using Agents.OpenAI;
using ExternalMedia;
using Infrastructure;
using LearningExtraction;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using UserInputInterfaces;

namespace Linguine.Tabs
{
    internal class TextualMediaViewerViewModel : TabViewModelBase
    {
        public ICommand LoadCommand { get; private set; }
        public ICommand DecomposeCommand { get; private set; }

        public String RawText
        {
            get => _rawText;
            private set
            {
                _rawText = value;
                OnPropertyChanged(nameof(RawText));
            }
        }

        public List<String> DiscoveredUnitsRaw
        {
            get => _discoveredUnits;
            private set
            {
                _discoveredUnits = value;
                OnPropertyChanged(nameof(DiscoveredUnitsRaw));
            }
        }
        public List<String> RootedUnits1
        {
            get => _discoveredUnits;
            private set
            {
                _discoveredUnits = value;
                OnPropertyChanged(nameof(RootedUnits1));
            }
        }


        public List<String> RootedUnits2
        {
            get => _discoveredUnits;
            private set
            {
                _discoveredUnits = value;
                OnPropertyChanged(nameof(RootedUnits2));
            }
        }

        public List<String> RootedUnits3
        {
            get => _discoveredUnits;
            private set
            {
                _discoveredUnits = value;
                OnPropertyChanged(nameof(RootedUnits3));
            }
        }



        public List<String> RootedUnits4
        {
            get => _discoveredUnits;
            private set
            {
                _discoveredUnits = value;
                OnPropertyChanged(nameof(RootedUnits4));
            }
        }

        public List<String> RootedUnits5
        {
            get => _discoveredUnits;
            private set
            {
                _discoveredUnits = value;
                OnPropertyChanged(nameof(RootedUnits5));
            }
        }

        private TextualMedia? _textualMedia;
        private TextualMediaLoader _loader;

        private TextDecomposition? _injectiveDecomposition;
        private TextDecomposition? _caseNormalisedDecomposition;
        private TextDecomposition? _rootedDecomposition;

        private String _rawText;
        private List<String> _discoveredUnits;

        public TextualMediaViewerViewModel(UIComponents uiComponents, MainModel parent) : base(uiComponents, parent)
        {
            Title = "Text Viewer";

            _loader = new TextualMediaLoader(uiComponents.CanVerify, uiComponents.CanChooseFromList);

            LoadCommand = new RelayCommand(() => Load());
            DecomposeCommand = new RelayCommand(() => Decompose());
        }

        private async Task Decompose()
        {
            if (_textualMedia is null)
            {
                _uiComponents.CanMessage.Show("Please load text first");
                return;
            }

            if (_mainModel.TextDecomposer is null)
            {
                if (!_mainModel.LoadTextDecompositionService())
                {
                    _uiComponents.CanMessage.Show("Text decomposition service loading failed");
                    return;
                }
            }
            try
            {

                // TODO - this is domain logic emerging, should be put in the model

                _injectiveDecomposition =      await _mainModel.TextDecomposer?.DecomposeText(_textualMedia, mustInject: true);
                DiscoveredUnitsRaw                 = _injectiveDecomposition?.Flattened().Units?.Select(td => td.Total.Text).ToList() ?? new List<string>();

                //_caseNormalisedDecomposition = await _mainModel.CaseNormaliser?.NormaliseCases(_injectiveDecomposition);
                //DiscoveredUnitsCaseNormalised = _caseNormalisedDecomposition?.Flattened().Units?.Select(td => td.Total.Text).ToList() ?? new List<string>();


                // best so far
                List<double> Temps = new List<double> { 1.0, 1.0, 1.4, 1.6, 1.8 };
                List<double> TopPs = new List<double> { 0.5, 0.5, 0.3, 0.2, 0.1 };


                _mainModel.UnitRooter?.SetTemperature(Temps[0]);
                _mainModel.UnitRooter?.SetTopP(TopPs[0]);


                _caseNormalisedDecomposition = await _mainModel.UnitRooter?.RootUnits(_injectiveDecomposition);
                //_caseNormalisedDecomposition = await DecompositionTransformer.ApplyAgent(new LowercasingAgent(), _injectiveDecomposition, 100000, 100);
                RootedUnits1 = _caseNormalisedDecomposition?.Flattened().Units?.Select(td => td.Total.Text).ToList() ?? new List<string>();

                _mainModel.UnitRooter?.SetTemperature(Temps[1]);
                _mainModel.UnitRooter?.SetTopP(TopPs[1]);


                _rootedDecomposition = await _mainModel.UnitRooter?.RootUnits(_caseNormalisedDecomposition /*_rootedDecomposition*/);
                RootedUnits2                 = _rootedDecomposition?.Flattened().Units?.Select(td => td.Total.Text).ToList() ?? new List<string>();

                
                _mainModel.UnitRooter?.SetTemperature(Temps[2]);
                _mainModel.UnitRooter?.SetTopP(TopPs[2]);

                _caseNormalisedDecomposition = await _mainModel.UnitRooter?.RootUnits(_rootedDecomposition);
                RootedUnits3 = _caseNormalisedDecomposition?.Flattened().Units?.Select(td => td.Total.Text).ToList() ?? new List<string>();


                _mainModel.UnitRooter?.SetTemperature(Temps[3]);
                _mainModel.UnitRooter?.SetTopP(TopPs[3]);

                _caseNormalisedDecomposition = await _mainModel.UnitRooter?.RootUnits(_rootedDecomposition);
                RootedUnits4 = _caseNormalisedDecomposition?.Flattened().Units?.Select(td => td.Total.Text).ToList() ?? new List<string>();

                _mainModel.UnitRooter?.SetTemperature(Temps[4]);
                _mainModel.UnitRooter?.SetTopP(TopPs[4]);

                _caseNormalisedDecomposition = await _mainModel.UnitRooter?.RootUnits(_rootedDecomposition);
                RootedUnits5 = _caseNormalisedDecomposition?.Flattened().Units?.Select(td => td.Total.Text).ToList() ?? new List<string>();

                

                // TODO - some sort of annealing process, until the unit rooter stops changing the text

            }
            catch (AggregateException ae)
            {
                // flatten the AggregateException to make it easier to handle individual exceptions
                ae.Flatten().Handle(ex =>
                {
                    if (ex is ApiException apiEx)
                    {
                        _uiComponents.CanMessage.Show($"Error calling API: {apiEx.Message}\nStatus Code: {apiEx.StatusCode}");
                        return true; 
                    }
                    return false; // indicate that we haven't handled other types of exceptions
                });
            }
            catch (Exception ex)
            {
                _uiComponents.CanMessage.Show($"An unexpected error occurred: {ex.Message}");
            }
        }

        private void Load()
        {
            if (_textualMedia is not null)
            {
                if (!_uiComponents.CanVerify.AskYesNo("Media already loaded, load a new one?"))
                {
                    return;
                }
            }

            String filename = _uiComponents.CanBrowseFiles.Browse();

            try
            {
                _textualMedia = _loader.LoadFromFile(filename, ConfigManager.TargetLanguage);
                RawText = _textualMedia.Text;
            }
            catch (Exception e)
            {
                _uiComponents.CanMessage.Show("media loading aborted");
            }
        }
    }
}

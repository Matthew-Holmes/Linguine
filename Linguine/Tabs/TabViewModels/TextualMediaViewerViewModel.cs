using Agents.OpenAI;
using ExternalMedia;
using Infrastructure;
using LearningExtraction;
using System;
using System.Collections.Generic;
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
        public List<String> DiscoveredUnitsRooted
        {
            get => _discoveredUnits;
            private set
            {
                _discoveredUnits = value;
                OnPropertyChanged(nameof(DiscoveredUnitsRooted));
            }
        }

        private TextualMedia? _textualMedia;
        private TextualMediaLoader _loader;
        private TextDecomposition? _injectiveDecomposition;
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
                _injectiveDecomposition =      await _mainModel.TextDecomposer?.DecomposeText(_textualMedia, mustInject: true);
                DiscoveredUnitsRaw                 = _injectiveDecomposition?.Flattened().Decomposition?.Select(td => td.Total.Text).ToList() ?? new List<string>();

                _rootedDecomposition = await _mainModel.UnitRooter?.RootUnits(_injectiveDecomposition);
                DiscoveredUnitsRooted = _rootedDecomposition?.Flattened().Decomposition?.Select(td => td.Total.Text).ToList() ?? new List<string>();
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

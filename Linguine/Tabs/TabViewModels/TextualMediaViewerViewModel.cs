using Agents.OpenAI;
using ExternalMedia;
using Infrastructure;
using LearningExtraction;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public ICommand ResolveRootsCommand { get; private set; }

        public String RawText
        {
            get => _rawText;
            private set
            {
                _rawText = value;
                OnPropertyChanged(nameof(RawText));
            }
        }

        public List<String> DiscoveredUnits
        {
            get => _discoveredUnits;
            private set
            {
                _discoveredUnits = value;
                OnPropertyChanged(nameof(DiscoveredUnits));
            }
        }

        public List<String> DiscoveredRoots
        {
            get => _discoveredRoots;
            private set
            {
                _discoveredRoots = value;
                OnPropertyChanged(nameof(DiscoveredRoots));
            }
        }
        
        private TextualMedia? _textualMedia;
        private TextualMediaLoader _loader;
        private TextDecomposition? _decomposition;
        private TextDecomposition? _rootsDecomposition;
        private String _rawText;
        private List<String> _discoveredUnits;
        private List<String> _discoveredRoots;

        public TextualMediaViewerViewModel(UIComponents uiComponents, MainModel parent) : base(uiComponents, parent)
        {
            Title = "Text Viewer";

            _loader = new TextualMediaLoader(uiComponents.CanVerify, uiComponents.CanChooseFromList);

            LoadCommand = new RelayCommand(() => Load());
            DecomposeCommand = new RelayCommand(() => DecomposeAndRoot());
            //ResolveRootsCommand = new RelayCommand(() => ResolveRoots());
        }

        /*
        private async Task ResolveRoots()
        {
            if (_textualMedia is null)
            {
                _uiComponents.CanMessage.Show("Please load text first");
                return;
            }

            if (DiscoveredUnits.Count == 0)
            {
                _uiComponents.CanMessage.Show("Please decompose text first");
                return;
            }

            if (_mainModel.RootResolver is null)
            {
                if (!await _mainModel.LoadRootResolutionService())
                {
                    _uiComponents.CanMessage.Show("Root resolution service loading failed");
                    return;
                }
            }

            try
            {
                _rootsDecomposition = await _mainModel.RootResolver?.ResolveRoots(_decomposition);
                DiscoveredRoots = _rootsDecomposition?.Flattened().Units?.Select(td => td.Total.Text).ToList() ?? new List<string>();
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
        */

        
        private async Task DecomposeAndRoot()
        {
            if (_textualMedia is null)
            {
                _uiComponents.CanMessage.Show("Please load text first");
                return;
            }

            if (_mainModel.TextDecomposer is null)
            {
                if (!_mainModel.LoadTextDecompositionAndRootingService())
                {
                    _uiComponents.CanMessage.Show("Text decomposition service loading failed");
                    return;
                }
            }
            try
            {
                (_decomposition,_rootsDecomposition) = await _mainModel.TextDecomposerAndRooter?.DecomposeText(_textualMedia, mustInject: true);
                DiscoveredUnits =      _decomposition?.Flattened().Units?.Select(td => td.Total.Text).ToList() ?? new List<string>();
                DiscoveredRoots = _rootsDecomposition?.Flattened().Units?.Select(td => td.Total.Text).ToList() ?? new List<string>();
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

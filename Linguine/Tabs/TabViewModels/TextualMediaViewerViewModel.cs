//using Agents.OpenAI;
using ExternalMedia;
using Infrastructure;
//using LearningExtraction;
using LearningStore;
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
        /*
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

        public List<List<Tuple<String,bool>>> PossibleDefinitionStringsForRooted
        {
            get => _possibleDefinitionStringsForRooted;
            private set
            {
                // so that the app won't get confused
                _correctDefinitionIndex = Enumerable.Repeat(-1, value.Count).ToList();

                _possibleDefinitionStringsForRooted = value;
                OnPropertyChanged(nameof(PossibleDefinitionStringsForRooted));
            }
        }

        private TextualMedia? _textualMedia;
        private TextualMediaLoader _loader;
        private TextDecomposition? _injectiveDecomposition;
        private TextDecomposition? _rootedDecomposition;
        private String _rawText;
        private List<String> _discoveredUnits;

        private List<List<DictionaryDefinition>> _possibleDefinitionsForRooted;
        private List<List<Tuple<String, bool>>> _possibleDefinitionStringsForRooted;
        private List<int> _correctDefinitionIndex;

 
        */
        public TextualMediaViewerViewModel(UIComponents uiComponents, MainModel parent) : base(uiComponents, parent)
        {
            Title = "Text Viewer";
            /*
            _loader = new TextualMediaLoader(uiComponents.CanVerify, uiComponents.CanChooseFromList);

            LoadCommand = new RelayCommand(() => Load());
            DecomposeCommand = new RelayCommand(() => Decompose());
            */
        }
        /*

        private async Task Decompose()
        {
            if (!PrepareToDecompose()) { return; }

            try
            {
                _injectiveDecomposition = await _mainModel.TextDecomposer?.DecomposeText(_textualMedia, mustInject: true);
                DiscoveredUnitsRaw    = _injectiveDecomposition?.Flattened().Decomposition?.Select(td => td.Total.Text).ToList() ?? new List<string>();

                _rootedDecomposition =    await _mainModel.UnitRooter?.RootUnits(_injectiveDecomposition);
                DiscoveredUnitsRooted = _rootedDecomposition?.Flattened().Decomposition?.Select(td => td.Total.Text).ToList() ?? new List<string>();


                _possibleDefinitionsForRooted = _mainModel.DefinitionResolver?.GetPossibleDefinitions(_rootedDecomposition);
                
                PossibleDefinitionStringsForRooted = _possibleDefinitionsForRooted.Select(
                    defList => defList.Select(
                        def => Tuple.Create(def.Definition, false)).ToList()
                                             ).ToList();

                _correctDefinitionIndex = await _mainModel.DefinitionResolver?.IdentifyCorrectDefinitions(
                    _possibleDefinitionsForRooted,
                    _rootedDecomposition,
                    _injectiveDecomposition) ?? new List<int>();

                List<List<Tuple<String, bool>>> tmp = new List<List<Tuple<string, bool>>>();

                for (int i = 0; i != _correctDefinitionIndex.Count; i++)
                {
                    tmp.Add(new List<Tuple<string, bool>>());

                    for (int j = 0; j != PossibleDefinitionStringsForRooted[i].Count; j++)
                    {
                        if (j == _correctDefinitionIndex[i])
                        {
                            tmp[i].Add(Tuple.Create(PossibleDefinitionStringsForRooted[i][j].Item1, true));
                        }
                        else
                        {
                            tmp[i].Add(Tuple.Create(PossibleDefinitionStringsForRooted[i][j].Item1, false));
                        }
                    }
                }
                PossibleDefinitionStringsForRooted = tmp;

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

        private bool PrepareToDecompose()
        {
            if (_textualMedia is null)
            {
                _uiComponents.CanMessage.Show("Please load text first");
                return false;
            }

            if (_mainModel.TextDecomposer is null)
            {
                if (!_mainModel.LoadTextDecompositionService())
                {
                    _uiComponents.CanMessage.Show("Text decomposition service loading failed");
                    return false;
                }
            }

            if (_mainModel.DefinitionResolver is null)
            {
                var options = ExternalDictionaryManager.AvailableDictionaries(ConfigManager.TargetLanguage);

                if (options.Count == 0)
                {
                    _uiComponents.CanMessage.Show("No dictionaries available for target language!");
                    return false;
                }
                else if (options.Count > 1)
                {
                    throw new NotImplementedException();
                }

                // TODO - this is just a stopgap, ultimately will need to decide how to deal with the user changing the Target language
                // TODO++ and what to do if there are multiple dictionaries loaded for that language (primary/secondary etc?)
                if (!_mainModel.LoadDefinitionResolutionService(ExternalDictionaryManager.GetDictionary(ConfigManager.TargetLanguage, options.First())))
                {
                    _uiComponents.CanMessage.Show("Definition resolution service loading failed");
                    return false;
                }
            }

            return true;
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
        */
    }
}

using DataClasses;
using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using UserInputInterfaces;

namespace Linguine.Tabs
{
    class DefinitionResolverViewModel : TabViewModelBase
    {
        private Tuple<string, string, string>?         _contexts;
        private Tuple<string, string, string, string>? _bestTranslatedAsText;

        private int       _defIndex;
        private String    _wordText;
        private String    _rootedWordText;
        private Statement _statement;
        private String    _statementTranslationText;

        private List<int>    _existingDefinitionsKeys          = new List<int>();
        private List<String> _existingDefinitionTexts          = new List<string>();

        private bool _showWaitingForDefinitions      = true;
        private bool _showExistingDefinitions        = false;
        private bool _showNoExistingDefinitionsFound = false;
        private bool _showIAmStillStuckButton        = false;

        public ICommand GenerateExplanationsCommand { get; set; }

        public ICommand SelectDefinitionCommand { get; set; }
        private String  _selectedDefinition     { get; set; }


        public bool _haveStatementTranslation  = false;
        public bool _haveSingleWordTranslation = false;
        public bool _haveSelectedDefinition    = false;
        public bool ShowResolveButton => _haveSelectedDefinition && _haveSingleWordTranslation && _haveStatementTranslation && !ResolutionComplete;

        public ICommand ResolveCommand   { get; set; }
        private bool _resolutionComplete = false;

        public DefinitionResolverViewModel(Statement selectedStatement, int defIndex,
            UIComponents uiComponents, MainModel model, MainViewModel parent) : base(uiComponents, model, parent)
        {
            Title = "Resolve Definition";

            Context = StatementHelper.RunFromStatementDefIndex(selectedStatement, defIndex);

            _statement      = selectedStatement;
            _defIndex       = defIndex;
            _wordText       = selectedStatement.InjectiveDecomposition.Units[defIndex].Trim();
            _rootedWordText = selectedStatement.RootedDecomposition.Units[defIndex].Trim();

            SelectDefinitionCommand     = new RelayCommand<String>(OnSelectDefinition);
            ResolveCommand              = new RelayCommand(() => Resolve());
            GenerateExplanationsCommand = new RelayCommand(() => GenerateExplanations());


            Task.Run(() => GoAndGetDetailsForUser());
            Task.Run(() => GetExistingDefinitionOptions());          
        }

        private void GenerateExplanations()
        {
            // update the existing definition translations with the information from the explanations
            throw new NotImplementedException();
        }

        private void Resolve()
        {
            int selectedIndex = ExistingDefinitions.IndexOf(SelectedDefinition);
            int selectedDefID = _existingDefinitionsKeys[selectedIndex];

            if (_uiComponents.CanVerify.AskYesNo($"Proceed with resolving the definition to be \"{SelectedDefinition}\"?"))
            {
                bool success = _model.ResolveDefinition(_statement, _defIndex, selectedDefID);
                if (!success)
                {
                    _uiComponents.CanMessage.Show("something went wrong, check the logs");
                }
                else
                {
                    ResolutionComplete = true;
                    OnPropertyChanged(nameof(ShowResolveButton));
                }
            }
        }

        private void OnSelectDefinition(String selectedDef)
        {
            if (ResolutionComplete) { return; }
            // for the highlighting in the UI
            if (selectedDef == SelectedDefinition)
            {
                SelectedDefinition      = ""; // click again to unselect
                _haveSelectedDefinition = false; OnPropertyChanged(nameof(ShowResolveButton));
            }
            else
            {
                SelectedDefinition      = selectedDef;
                _haveSelectedDefinition = true; OnPropertyChanged(nameof(ShowResolveButton));
            }
        }


        #region UI properties

        public bool ShowIAmStillStuckButton
        {
            get => _showIAmStillStuckButton;
            set
            {
                _showIAmStillStuckButton = value;
                OnPropertyChanged(nameof(ShowIAmStillStuckButton));
            }
        }
        public bool ResolutionComplete
        {
            get => _resolutionComplete;
            set
            {
                _resolutionComplete = value;
                OnPropertyChanged(nameof(ResolutionComplete));
            }
        }

        public String SelectedDefinition
        {
            get => _selectedDefinition;
            set
            {
                _selectedDefinition = value;
                OnPropertyChanged(nameof(SelectedDefinition));
            }
        }

        public Tuple<string, string, string> Context
        {
            get => _contexts;
            set
            {
                _contexts = value;
                OnPropertyChanged(nameof(Context));
            }
        }

        public String StatementTranslation
        {
            get => _statementTranslationText;
            set
            {
                _statementTranslationText = value;
                OnPropertyChanged(nameof(StatementTranslation));
            }
        }

        public Tuple<string, string, string, string> BestTranslatedAsTexts
        {
            get => _bestTranslatedAsText;
            set
            {
                _bestTranslatedAsText = value;
                OnPropertyChanged(nameof(BestTranslatedAsTexts));
            }
        }

        public List<String> ExistingDefinitions
        {
            get => _existingDefinitionTexts;
            set
            {
                _existingDefinitionTexts = value;
                OnPropertyChanged(nameof(ExistingDefinitions));
            }
        }

        public bool ShowWaitingForDefinitions
        {
            get => _showWaitingForDefinitions;
            set
            {
                _showWaitingForDefinitions = value;
                OnPropertyChanged(nameof(ShowWaitingForDefinitions));
            }
        }

        public bool ShowNoExistingDefinitionsFound
        {
            get => _showNoExistingDefinitionsFound;
            set
            {
                _showNoExistingDefinitionsFound = value;
                OnPropertyChanged(nameof(ShowNoExistingDefinitionsFound));
            }
        }

        public bool ShowExistingDefinitions
        {
            get => _showExistingDefinitions;
            set
            {
                _showExistingDefinitions = value;
                OnPropertyChanged(nameof(ShowExistingDefinitions));
            }
        }

        #endregion

        private async Task GoAndGetDetailsForUser()
        {
            StatementTranslation      = await _model.GetStatementTranslationText(_statement);
            _haveStatementTranslation = true;
            
            string bestWordTranslation  = await _model.GetBestWordTranslation(_statement, _defIndex);
            bestWordTranslation         = bestWordTranslation.Trim();

            ForBestTranslation fixedParts = TextFactory.BestTranslatedAsString(_model.Native);

            BestTranslatedAsTexts      = Tuple.Create(fixedParts.inThisStatementIThink, _wordText, fixedParts.isBestTranslatedAs, bestWordTranslation);
            _haveSingleWordTranslation = true;

            OnPropertyChanged(nameof(ShowResolveButton));
        }

        private async Task GetExistingDefinitionOptions()
        {
            List<Tuple<int, String>> existing = await _model.GetExistingDefinitionKeysAndTexts(_rootedWordText);

            ShowWaitingForDefinitions = false;

            if (existing.Count > 0)
            {
                ShowExistingDefinitions = true;
            } 
            else
            {
                ShowNoExistingDefinitionsFound = true;
            }

            _existingDefinitionsKeys = existing.Select(t => t.Item1).ToList();
            ExistingDefinitions      = existing.Select(t => t.Item2).ToList();

            Thread.Sleep(5_000);

            ShowIAmStillStuckButton = true;
        }
    }
}

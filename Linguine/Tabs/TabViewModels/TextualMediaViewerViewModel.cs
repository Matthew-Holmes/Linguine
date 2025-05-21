using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using UserInputInterfaces;
using DataClasses;

namespace Linguine.Tabs
{
    public class TextualMediaViewerViewModel : TabViewModelBase
    {
        public readonly int SessionID;
        public String FullText { get; set; }

        public List<Statement> TextStatements;

        // the user can then select a unit to see its details
        public ICommand UnitSelectedCommand { get; internal set; }

        // perform a processing step
        public ICommand ProcessChunkCommand { get; set; }

        // invoke this to trigger a redraw, e.g. once a processing step
        // has been completed
        public event EventHandler UnderlyingStatementsChanged;


        private string _selectedUnitText;
        private string _selectedUnitDefinition;
        private string _selectedUnitRootedText;
        private string _selectedUnitRootedPronunciation;
        private ObservableCollection<string> _selectedUnitContextInfo;
        private bool _showSaveWordButton;
        private string _selectedUnitParsedDefinition;

        private bool _showResolveDefinitionButton = false;
        private bool _showRepairDefinitionButton  = false;
        private bool _showWrongDefinitionButton;

        public ICommand ResolveSelectedDefinitionCommand { get; init; }
        public ICommand RepairSelectedDefinitionCommand  { get; init; }
        public ICommand WrongDefinitionCommand           { get; init; }

        // right hand pane unit properties
        public String SelectedUnitText
        {
            get => _selectedUnitText;
            set
            {
                _selectedUnitText = value;
                OnPropertyChanged(nameof(SelectedUnitText));
            }
        }

        public String SelectedUnitRootedText
        {
            get => _selectedUnitRootedText;
            set
            {
                _selectedUnitRootedText = value;
                OnPropertyChanged(nameof(SelectedUnitRootedText));
            }
        }

        public String SelectedUnitRootedPronunciation
        {
            get => _selectedUnitRootedPronunciation;
            set
            {
                _selectedUnitRootedPronunciation = value;
                OnPropertyChanged(nameof(SelectedUnitRootedPronunciation));
            }
        }

        public String SelectedUnitDefinitionText
        {
            get => _selectedUnitDefinition;
            set
            {
                _selectedUnitDefinition = value;
                OnPropertyChanged(nameof(SelectedUnitDefinitionText));
            }
        }

        public String SelectedUnitParsedDefinitionText
        {
            get => _selectedUnitParsedDefinition;
            set
            {
                _selectedUnitParsedDefinition = value;
                OnPropertyChanged(nameof(SelectedUnitParsedDefinitionText));
            }
        }

        public ObservableCollection<String> SelectedUnitContextInfo
        {
            get => _selectedUnitContextInfo;
            set
            {
                _selectedUnitContextInfo = value;
                OnPropertyChanged(nameof(SelectedUnitContextInfo));
            }
        }


        public bool ShowResolveDefinitionButton
        {
            get => _showResolveDefinitionButton;
            private set
            {
                _showResolveDefinitionButton = value;
                OnPropertyChanged(nameof(ShowResolveDefinitionButton));
            }
        }
        public bool ShowRepairDefinitionButton
        {
            get => _showRepairDefinitionButton;
            private set
            {
                _showRepairDefinitionButton = value;
                OnPropertyChanged(nameof(ShowRepairDefinitionButton));
            }
        }

        public bool ShowWrongDefinitionButton
        {
            get => _showWrongDefinitionButton;
            private set
            {
                _showWrongDefinitionButton = value;
                OnPropertyChanged(nameof(ShowWrongDefinitionButton));
            }
        }

        public bool ShowSaveWordButton
        {
            get => _showSaveWordButton;
            set
            {
                _showSaveWordButton = value;
                OnPropertyChanged(nameof(ShowSaveWordButton));

            }
        }

        public ICommand SaveWordCommand { get; private set; }
        public ICommand ExportLearnerListCommand { get; private set; }

        public TextualMediaViewerViewModel(TextualMedia tm, UIComponents uiComponents, MainModel model, MainViewModel parent) 
            : base(uiComponents, model, parent)
        {
            Title = "Text Viewer";
            TabClosed += (s,e) => model.CloseSession(tm);

            FullText = tm.Text;

            UnitSelectedCommand = new RelayCommand<Tuple<int, int>>(OnUnitSelected);

            ProcessChunkCommand = new RelayCommand(async () => await ProcessChunk());

            ShowSaveWordButton = false;
            SaveWordCommand          = new RelayCommand(() => SaveSelectedUnit());
            ExportLearnerListCommand = new RelayCommand(() => ExportLearnerListToCsv());
            
            RepairSelectedDefinitionCommand  = new RelayCommand(() => RepairSelectedDefinition());
            ResolveSelectedDefinitionCommand = new RelayCommand(() => ResolveSelectedDefinition());
            WrongDefinitionCommand           = new RelayCommand(() => WrongDefinition());


            List<Statement>? textStatements = _model.GetAllStatementsFor(tm);

            if (textStatements is null)
            {
                _uiComponents.CanMessage.Show("failed to access processed information for selected text");
            }

            TextStatements = textStatements;
        }

        private void WrongDefinition()
        {
            if (SelectedStatement is null)
            {
                _uiComponents.CanMessage.Show("failed to find surrounding statement for definition");
                return;
            }

            if (SelectedUnitIndex == -1)
            {
                _uiComponents.CanMessage.Show("failed to located definition in statement");
            }

            _parent.BeginDefinitionResolution(SelectedStatement, SelectedUnitIndex);
        }

        private void ResolveSelectedDefinition()
        {
            if (SelectedStatement is null)
            {
                _uiComponents.CanMessage.Show("failed to find surrounding statement for definition");
                return;
            }

            if (SelectedUnitIndex == -1)
            {
                _uiComponents.CanMessage.Show("failed to located definition in statement");
            }

            _parent.BeginDefinitionResolution(SelectedStatement, SelectedUnitIndex);
        }

        private void RepairSelectedDefinition()
        {
            if (SelectedUnitDefinition is null)
            {
                _uiComponents.CanMessage.Show("no definition selected!");
                return;
            }

            _parent.BeginDefinitionRepair(SelectedUnitDefinition);
        }
        private DictionaryDefinition? SelectedUnitDefinition { get; set; }

        private void SaveSelectedUnit()
        {
            if (SelectedUnitDefinition is null)
            {
                _uiComponents.CanMessage.Show("attempting to save null definition - aborting");
                return;
            }
            _model.AddLearnerListItem(SelectedUnitDefinition);    
        }

        private void ExportLearnerListToCsv()
        {
            if (_model.LearnerList.Count == 0)
            {
                _uiComponents.CanMessage.Show("please save some words");
                return;
            }

            string csv_out = _uiComponents.CanBrowseFiles.BrowseSaveFile(
                "export.csv", ".csv", "CSV files (*.csv)|*.csv|All files (*.*)|*.*");

            if (csv_out == null || csv_out == "")
            {
                _uiComponents.CanMessage.Show("no output file selected");
                return;
            }

            if (_model.ExportLearnerListToCSV(csv_out))
            {
                _uiComponents.CanMessage.Show("export successful");
            }
            else
            {
                _uiComponents.CanMessage.Show("export failed");
            }

        }

        private Statement? SelectedStatement { get; set; }
        private int        SelectedUnitIndex { get; set; } = -1;

        private void OnUnitSelected(Tuple<int, int> tuple)
        {
            int statementIndex = tuple.Item1;
            int unitIndex = tuple.Item2;

            Statement statement = TextStatements[statementIndex];

            SelectedStatement = statement;
            SelectedUnitIndex = unitIndex;

            // Warning - this won't work if the decomposition is multilevel
            SelectedUnitText        = statement.InjectiveDecomposition?.Units?[unitIndex] ?? "";
            SelectedUnitRootedText  = statement.RootedDecomposition?.Units?[unitIndex] ?? "";
            
            // TODO - get the parsed definition

            SelectedUnitDefinition = statement.RootedDecomposition?.Decomposition?[unitIndex].Definition;
            SelectedUnitDefinitionText = SelectedUnitDefinition?.Definition ?? "";
            SelectedUnitContextInfo = new ObservableCollection<String>(statement.StatementContext);


            if (SelectedUnitDefinition is null)
            {
                ShowResolveDefinitionButton = true;
                ShowRepairDefinitionButton  = false;
                ShowWrongDefinitionButton   = false;

                ShowSaveWordButton = false;
                SelectedUnitParsedDefinitionText = "";
                SelectedUnitRootedPronunciation = "";
            }
            else
            {
                ShowResolveDefinitionButton = false;
                ShowRepairDefinitionButton  = true;
                ShowWrongDefinitionButton   = true;

                ShowSaveWordButton = true;
                SelectedUnitParsedDefinitionText = _model.GetParsedDictionaryDefinition(SelectedUnitDefinition)?.ParsedDefinition
                        ?? "";
                SelectedUnitRootedPronunciation = SelectedUnitDefinition.RomanisedPronuncation ?? "";
            }
            //throw new NotImplementedException();
            // the logic to query the parsed definition from the model, and setup a callback if
            // it needs to be recomputed
        }

        private async Task ProcessChunk()
        {
            try
            {
                await _model.ProcessNextChunkForSession(SessionID);
            }
            catch (Agents.MissingAPIKeyException e)
            {
                _parent.HandleMissingApiKeys(e);
                await Task.FromResult(false);
            }

            UnderlyingStatementsChanged?.Invoke(this, new EventArgs());
        }
    }
}

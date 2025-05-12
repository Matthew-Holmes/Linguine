using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using UserInputInterfaces;
using DataClasses;
using System.Windows.Forms;
using System.Security.RightsManagement;

namespace Linguine.Tabs
{
    public class TextualMediaViewerViewModel : TabViewModelBase
    {
        public readonly int SessionID;
        public String FullText { get; set; }


        // ****************** UI Interaction Properties ************************
        // they use custom events, and are used to handle paging 
        // since only the UI will be able to tell how much of the text can
        // be displayed on the given display region

        public int LocalCursor;

        // UI handles these, can do multiple page steps if desired
        public event EventHandler<int> PageForwards;
        public event EventHandler<int> PageBackwards;

        // this is used by the UI to choose where to break the text
        public List<int> SortedStatementStartIndices;

        // the UI then invokes this when it has done paging
        public ICommand PageLocatedCommand { get; set; }

        // ViewModel then raises this once has pulled the statements covering
        // the page displayed
        public event EventHandler<List<Statement>> StatementsCoveringPageChanged;
        public List<Statement> StatementsCoveringPage;

        // the user can then select a unit to see its details
        public ICommand UnitSelectedCommand { get; internal set; }

        // perform a processing step
        public ICommand ProcessChunkCommand { get; set; }

        // invoke this to trigger a redraw, e.g. once a processing step
        // has been completed
        public event EventHandler<List<int>> UnderlyingStatementsChanged;

        // *********************************************************************        


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

        public TextualMediaViewerViewModel(int sessionId, UIComponents uiComponents, MainModel model, MainViewModel parent) 
            : base(uiComponents, model, parent)
        {
            Title = "Text Viewer";
            SessionID = sessionId;
            TabClosed += (s,e) => model.CloseSession(sessionId);

            FullText = model.GetFullTextFromSessionID(sessionId) ?? throw new Exception("couldn't find session");
            SortedStatementStartIndices =   model.GetSortedStatementStartIndicesFromSessionID(sessionId) 
                    ?? throw new Exception("couldn't find session");

            PageLocatedCommand  = new RelayCommand<Tuple<int, int>>(OnPageLocated);
            UnitSelectedCommand = new RelayCommand<Tuple<int, int>>(OnUnitSelected);
            LocalCursor = model.GetCursor(SessionID);

            SetupTraversalCommands();
            ProcessChunkCommand = new RelayCommand(async () => await ProcessChunk());

            ShowSaveWordButton = false;
            SaveWordCommand          = new RelayCommand(() => SaveSelectedUnit());
            ExportLearnerListCommand = new RelayCommand(() => ExportLearnerListToCsv());
            
            RepairSelectedDefinitionCommand  = new RelayCommand(() => RepairSelectedDefinition());
            ResolveSelectedDefinitionCommand = new RelayCommand(() => ResolveSelectedDefinition());
            WrongDefinitionCommand           = new RelayCommand(() => WrongDefinition());
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
            _uiComponents.CanMessage.Show("need to implement this");
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

            Statement statement = StatementsCoveringPage[statementIndex];

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

            SortedStatementStartIndices = _model.GetSortedStatementStartIndicesFromSessionID(SessionID)
                ?? throw new Exception("couldn't find session");

            UnderlyingStatementsChanged?.Invoke(this, SortedStatementStartIndices);
        }

        #region traversal
        public ICommand StepRightCommand { get; set; }
        public ICommand StepLeftCommand { get; set; }
        public ICommand BigStepRightCommand { get; set; }
        public ICommand BigStepLeftCommand { get; set; }

        private void SetupTraversalCommands()
        {
            StepLeftCommand = new RelayCommand(() => StepBack(1));
            StepRightCommand = new RelayCommand(() => StepForward(1));
            BigStepLeftCommand = new RelayCommand(() => StepBack(10));
            BigStepRightCommand = new RelayCommand(() => StepForward(10));
        }


        private void StepForward(int pages)
        {
            PageForwards?.Invoke(this, pages);
        }

        private void StepBack(int pages)
        {
            PageBackwards?.Invoke(this, pages);
        }

        private void OnPageLocated(Tuple<int,int> indices)
        {
            LocalCursor = indices.Item1;

            _model.CursorMoved(SessionID, indices.Item1); // now we are static can update the database

            List<Statement>? toUpdate = _model.GetStatementsCoveringRange(
                SessionID, indices.Item1, indices.Item2);

            if (toUpdate == null)
            {
                _uiComponents.CanMessage.Show("statements covering page call failed");
                return;
            }

            StatementsCoveringPage = toUpdate;

            StatementsCoveringPageChanged?.Invoke(this, StatementsCoveringPage);
        }
        #endregion
    }
}

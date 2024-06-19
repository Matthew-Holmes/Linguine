//using Agents.OpenAI;
using ExternalMedia;
using Infrastructure;
using Newtonsoft.Json.Linq;
using SQLitePCL;

//using LearningExtraction;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Printing;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Navigation;
using UserInputInterfaces;

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
        private ObservableCollection<string> _selectedUnitContextInfo;

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

        public String SelectedUnitDefinition
        {
            get => _selectedUnitDefinition;
            set
            {
                _selectedUnitDefinition = value;
                OnPropertyChanged(nameof(SelectedUnitDefinition));
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
        }

        private void OnUnitSelected(Tuple<int, int> tuple)
        {
            int statementIndex = tuple.Item1;
            int unitIndex = tuple.Item2;

            Statement statement = StatementsCoveringPage[statementIndex];

            // Warning - this won't work if the decomposition is multilevel
            SelectedUnitText        = statement.InjectiveDecomposition.Units[unitIndex];
            SelectedUnitRootedText  = statement.RootedDecomposition.Units[unitIndex];
            SelectedUnitDefinition  = statement.RootedDecomposition.Decomposition[unitIndex].Definition?.Definition ?? "";
            SelectedUnitContextInfo = new ObservableCollection<String>(statement.StatementContext);
        }

        private async Task ProcessChunk()
        {
            await _model.ProcessNextChunk(SessionID);

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

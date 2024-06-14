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
        int _pageDelta = 0;
        private List<Statement> _statementsCoveringPage;

        public String FullText { get; set; }

        private int _localCursor;
        public int LocalCursor
        {
            get => _localCursor;
            private set
            {
                _localCursor = value;
            }
        }

        // these are bound too by the view to do the UI view involved filling the display with text
        public event EventHandler<int> PageForwards;
        public event EventHandler<int> PageBackwards;

        // the view then invokes this when it has done paging
        public ICommand PageLocatedCommand { get; set; }

        // the view binds to this and uses it to markup the text
        public event EventHandler<List<Statement>> StatementsCoveringPageChanged;

        // these are all that are required for initial typesetting
        // once we have the range for a page, we can request the corresponding statements
        // these are more memory intensive so don't want to load all of them for the text
        // when we only need the few that are present on the visible page
        // (and maybe some for buffered pages ahead/behind)
        // TODO - the UI needs to have an event to know when these start indices change
        private List<int> _statementStartIndices;
        public List<int> SortedStatementStartIndices
        {
            get => _statementStartIndices;
            set
            {
                _statementStartIndices = value; 
                OnPropertyChanged(nameof(SortedStatementStartIndices));
            }
        }

        public List<Statement> StatementsCoveringPage
        {
            get => _statementsCoveringPage;
            set
            {
                _statementsCoveringPage = value; OnPropertyChanged(nameof(StatementsCoveringPage));
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

            PageLocatedCommand = new RelayCommand<Tuple<int, int>>(OnPageLocated);
            LocalCursor = model.GetCursor(SessionID);

            SetupTraversalCommands();
            ProcessChunkCommand = new RelayCommand(async () => await ProcessChunk());
        }

        private async Task ProcessChunk()
        {
            await _model.ProcessNextChunk(SessionID);
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

        public ICommand ProcessChunkCommand { get; set; }
    }
}

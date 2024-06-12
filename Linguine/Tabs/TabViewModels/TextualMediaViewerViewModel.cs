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
        private int _localCursor;
        private int _endOfPage;
        int _pageDelta = 0;
        private List<int> _statementStartIndices;
        private List<Statement> _statementsCoveringPage;

        public String FullText { get; set; }

        public int LocalCursor
        {
            get => _localCursor;
            private set
            {
                _localCursor = value; OnPropertyChanged(nameof(LocalCursor));
            }
        }

        public int EndOfPage
        {
            get => _endOfPage;
            private set
            {
                _localCursor = value; OnPropertyChanged(nameof(EndOfPage));
            }
        }


        // these are all that are required for initial typesetting
        // once we have the range for a page, we can request the corresponding statements
        // these are more memory intensive so don't want to load all of them for the text
        // when we only need the few that are present on the visible page
        // (and maybe some for buffered pages ahead/behind)
        public List<int> SortedStatementStartIndices
        {
            get => _statementStartIndices;
            set
            {
                _statementStartIndices = value; OnPropertyChanged(nameof(SortedStatementStartIndices));
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

        public ICommand PageLocated { get; set;}

        public TextualMediaViewerViewModel(int sessionId, UIComponents uiComponents, MainModel model, MainViewModel parent) 
            : base(uiComponents, model, parent)
        {
            Title = "Text Viewer";
            SessionID = sessionId;
            TabClosed += (s,e) => model.CloseSession(sessionId);

            FullText = model.GetFullTextFromSessionID(sessionId);
            _localCursor = model.GetCursor(SessionID);
            SortedStatementStartIndices = model.GetSortedStatementStartIndicesFromSessionID(sessionId);

            SetupTraversalCommands();

            PageLocated = new RelayCommand<Tuple<int,int>>(OnPageLocated);
            ProcessChunkCommand = new RelayCommand(async () => await ProcessChunk());

        }

        private void OnPageLocated(Tuple<int,int> indices)
        {
            _localCursor = indices.Item1;
            _endOfPage   = indices.Item2;

            // if we still have pages to turn, then don't do expensive statement lookup
            if (_pageDelta < 0)
            {
                StepBack(-1 * _pageDelta);
                return;
            }
            else if (_pageDelta > 0)
            {
                StepForward(_pageDelta);
                return;
            }

            PageChanged();
        }

        private void PageChanged()
        {
            _model.CursorMoved(SessionID, LocalCursor); // now we are static can update the database

            List<Statement>? toUpdate = _model.GetStatementsCoveringRange(
                SessionID, LocalCursor, EndOfPage);

            if (toUpdate == null)
            {
                _uiComponents.CanMessage.Show("statements covering page call failed");
                return;
            }

            StatementsCoveringPage = toUpdate;
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

        public event EventHandler PageForward;
        private void StepForward(int pages)
        {
            if (pages < 1) { return; }

            _pageDelta = pages - 1;

            PageForward?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler PageBackwards;
        private void StepBack(int pages)
        {
            if (pages < 1) { return; }

            _pageDelta = -1 * (pages - 1);

            PageBackwards?.Invoke(this, EventArgs.Empty);
        }

        private void SetupTraversalCommands()
        {
            StepLeftCommand = new RelayCommand(() => StepBack(1));
            StepRightCommand = new RelayCommand(() => StepForward(1));
            BigStepLeftCommand = new RelayCommand(() => StepBack(10));
            BigStepRightCommand = new RelayCommand(() => StepForward(10));
        }

        #endregion

        public ICommand ProcessChunkCommand { get; set; }
    }
}

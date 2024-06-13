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
                _localCursor = value; OnPropertyChanged(nameof(LocalCursor));
            }
        }

        private int _endOfPage;
        public int EndOfPage
        {
            get => _endOfPage;
            private set
            {
                _endOfPage = value; OnPropertyChanged(nameof(EndOfPage));
            }
        }


        // these are all that are required for initial typesetting
        // once we have the range for a page, we can request the corresponding statements
        // these are more memory intensive so don't want to load all of them for the text
        // when we only need the few that are present on the visible page
        // (and maybe some for buffered pages ahead/behind)
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

        public ICommand PageLocatedCommand { get; set;}

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
            _localCursor = model.GetCursor(SessionID);

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
            if (pages < 1 || EndOfPage >= FullText.Length - 1) { return; }

            _pageDelta = pages;

            if (LocalCursor == EndOfPage + 1)
            {
                LocalCursor++; // give jog if have got stuck for whatever reason
            }

            LocalCursor = EndOfPage + 1; // will trigger paging on the view, will call command when done
        }

        private void StepBack(int pages)
        {
            if (pages < 1 || LocalCursor <= 0) { return; }

            _pageDelta = -1 * pages;

            if (EndOfPage == LocalCursor - 1)
            { 
                EndOfPage--;
            }

            EndOfPage = LocalCursor - 1; // will trigger paging on the view, will call command when done
        }

        private void OnPageLocated(Tuple<int, int> indices)
        {
            _localCursor = indices.Item1;
            _endOfPage = indices.Item2;

            if (LocalCursor <= 0 || EndOfPage >= FullText.Length - 1 )
            { 
                // stop turning when we reach the end
            }
            else if (_pageDelta < -1 /* if we still have pages to turn, then don't do expensive statement lookup */)
            {
                _pageDelta++;

                if (LocalCursor > 0)
                {
                    EndOfPage = LocalCursor - 1; // will trigger paging on the view, will call command when done
                }

                return;
            }
            else if (_pageDelta > 1)
            {
                _pageDelta--;

                if (EndOfPage < FullText.Length - 1)
                {
                    LocalCursor = EndOfPage + 1; // will trigger paging on the view, will call command when done
                }
                return;
            }

            _pageDelta = 0;

            PageChanged(); // this only calls once we have no delta, so not loading statements for no reason
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


        #endregion

        public ICommand ProcessChunkCommand { get; set; }
    }
}

//using Agents.OpenAI;
using ExternalMedia;
using Infrastructure;
//using LearningExtraction;
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
    public class TextualMediaViewerViewModel : TabViewModelBase
    {
        public readonly int SessionID;
        private int _charsToDisplay = 1000;
        private int _linesToDisplay = 20;
        private int _localCursor;

        private String _fullText;
        private string _textViewWindow;

        public String TextViewWindow
        {
            get => _textViewWindow;
            set
            {
                _textViewWindow = value;
                OnPropertyChanged(nameof(TextViewWindow));
            }
        }


        public TextualMediaViewerViewModel(int sessionId, UIComponents uiComponents, MainModel model, MainViewModel parent) 
            : base(uiComponents, model, parent)
        {
            Title = "Text Viewer";
            SessionID = sessionId;
            TabClosed += (s,e) => model.CloseSession(sessionId);

            String? loaded = model.GetFullTextFromSessionID(sessionId);

            if (loaded is null)
            {
                _uiComponents.CanMessage.Show("loading text from session id failed");
                loaded = "";
            }

            _fullText = loaded;
            _localCursor = model.GetCursor(SessionID);

            Step(0);

            SetupTraversalCommands();

        }

        #region traversal
        public ICommand StepRightCommand { get; set; }
        public ICommand StepLeftCommand { get; set; }
        public ICommand BigStepRightCommand { get; set; }
        public ICommand BigStepLeftCommand { get; set; }

        private void Step(int pages)
        {
            // check don't exceed line limits
            int lhs = Math.Max(Math.Min(_localCursor, _localCursor + pages * _charsToDisplay), 0);
            int rhs = Math.Min(Math.Max(_localCursor, _localCursor + pages * _charsToDisplay), _fullText.Length);

            List<String> linesMoved = _fullText.Substring(lhs, rhs - lhs).Split('\n').ToList();

            if (linesMoved.Count > pages * _linesToDisplay)
            {
                if (pages < 0)
                {
                    linesMoved.Reverse();
                }
                int newStep = linesMoved.Take(Math.Abs(pages) * _linesToDisplay).Sum(s => s.Length);
                _localCursor += Math.Sign(pages) * newStep;
            }
            else
            {
                _localCursor += pages * _charsToDisplay;
            }

            _localCursor = Math.Min(_fullText.Length - _charsToDisplay, _localCursor);
            _localCursor = Math.Max(0, _localCursor);

            int windowEnd = Math.Min(_fullText.Length, _localCursor +  _charsToDisplay);

            string tmp = _fullText.Substring(_localCursor, windowEnd - _localCursor);

            List<String> tmpLines = tmp.Split('\n').ToList();

            if (tmpLines.Count > _linesToDisplay)
            {
                tmp = String.Join('\n', tmpLines.Take(_linesToDisplay));
            }

            TextViewWindow = tmp;

            if (!_model.CursorMoved(SessionID, _localCursor))
            {
                _uiComponents.CanMessage.Show("Updating cursor position failed");
            }
        }

        private void SetupTraversalCommands()
        {
            StepLeftCommand = new RelayCommand(() => Step(-1));
            StepRightCommand = new RelayCommand(() => Step(1));
            BigStepLeftCommand = new RelayCommand(() => Step(-10));
            BigStepRightCommand = new RelayCommand(() => Step(10));
        }

        #endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using UserInputInterfaces;
using System.Windows;
using System.Text.RegularExpressions;
using System.Media;
using System.IO;

namespace Linguine.Tabs
{
    class TestLearningViewModel : TabViewModelBase
    {
        public ICommand SubmitAnswerCommand        { get; private set; }
        public ICommand SubmissionCorrectCommand   { get; private set; }
        public ICommand SubmissionIncorrectCommand { get; private set; }
        public ICommand SetFocusCommand            { get; private set; }
        public ICommand NextContextCommand         { get; private set; }
        public ICommand PreviousContextCommand     { get; private set; }
        public ICommand PlayCurrentSoundCommand    { get; private set; }

        public bool ShowPlayCurrentSoundButton
        {
            get => _showPlayCurrentSoundButton;
            set
            {
                _showPlayCurrentSoundButton = value;
                OnPropertyChanged(nameof(ShowPlayCurrentSoundButton));
            }
        }

        private SoundPlayer? SoundPlayer { get; set; } = null;

        private bool _isVocabTest       { get; set; }
        private int _vocabTestRemaining { get; set; }

        private DateTime _posed;
        private DateTime _answered;
        private DateTime _finished;

        private List<int> DefIdsToLearn { get; set; }

        private void SetFocus(object param)
        {
            if (param is Button button)
            {
                Application.Current.Dispatcher.InvokeAsync(() => button.Focus());
            }
        }

        public TestLearningViewModel(UIComponents  uiComponents, 
                                     MainModel     model, 
                                     MainViewModel parent,
                                     bool          isVocabTest = false) 
            : base(uiComponents, model, parent)
        {
            Title = "Learn";

            if (isVocabTest)
            {
                SetupVocabTest();
            }

            SubmitAnswerCommand        = new RelayCommand(() => SubmitAnswer());
            SubmissionCorrectCommand   = new RelayCommand(() => HandleAnswerWasCorrect());
            SubmissionIncorrectCommand = new RelayCommand(() => HandleAnswerWasIncorrect());
            SetFocusCommand            = new RelayCommand<object>(SetFocus);

            PreviousContextCommand = new RelayCommand(() => PreviousContext());
            NextContextCommand     = new RelayCommand(() => NextContext());

            PlayCurrentSoundCommand = new RelayCommand(() => SoundPlayer?.Play());

            Reset();
        }

        private void NextContext()
        {
            _currentContextId += 1;
            _currentContextId = _currentContextId % Math.Max(Contexts.Count, 1);

            CurrentContext = Contexts.Count != 0 ? Contexts[_currentContextId] : Tuple.Create("", "", "");
        }

        private void PreviousContext()
        {
            _currentContextId -= 1;
            _currentContextId += Contexts.Count; // avoid remainder not mod issue
            _currentContextId = _currentContextId % Math.Max(Contexts.Count, 0);

            CurrentContext = Contexts.Count != 0 ? Contexts[_currentContextId] : Tuple.Create("", "", "");
        }

        private void SetupVocabTest()
        {
            _isVocabTest = true;
            _vocabTestRemaining = _model.DefLearning?.VocabTestWordCount ?? throw new Exception("couldn't access definition learning service");

            _vocabTestRemaining -= _model.DistintWordsTested;

            Title = $"Remaining: {_vocabTestRemaining}";
        }

        private String _prompt;
        private String _correctAnswer;
        private String _usersAnswer;
        private bool   _answerSubmitted = false;
        private bool  _allowSubmission = true;


        private DefinitionForTesting _definitionForTesting;
        private List<Tuple<string, string, string>> _contexts;
        private Tuple<string, string, string> _currentContext;
        private int _currentContextId = 0;
        private bool _showPlayCurrentSoundButton;

        public List<Tuple<string, string, string>> Contexts
        {
            get => _contexts;
            set
            {
                _contexts = value;
                OnPropertyChanged(nameof(Contexts));

                if (Contexts.Count > 0)
                {
                    CurrentContext = Contexts.FirstOrDefault();
                } else
                {
                    CurrentContext = null;
                }

                _currentContextId = 0;
            }
        }

        public Tuple<string, string, string> CurrentContext
        {
            get => _currentContext;
            set
            {
                _currentContext = value;
                OnPropertyChanged(nameof(CurrentContext));
            }
        }
       

        public String Prompt
        {
            get => _prompt;
            set
            {
                _prompt = value;
                OnPropertyChanged(nameof(Prompt));
            }
        }

        public String CorrectAnswer
        {
            get => _correctAnswer;
            set
            {
                _correctAnswer = value;
                OnPropertyChanged(nameof(CorrectAnswer));
            }
        }

        public String UserAnswer
        {
            get => _usersAnswer;
            set 
            { 
                _usersAnswer = value;
                OnPropertyChanged(nameof(UserAnswer));
            }
        }

        public bool AnswerSubmitted 
        {   get => _answerSubmitted;
            set 
            { 
                _answerSubmitted = value;
                OnPropertyChanged(nameof(AnswerSubmitted));
            }
        }

        public bool AllowSubmission
        {
            get => _allowSubmission;
            set 
            { 
                _allowSubmission = value;
                OnPropertyChanged(nameof(AllowSubmission));
            }
        }



        private void SubmitAnswer()
        {
            if (UserAnswer == "" || UserAnswer is null)
            {
                SoundPlayer?.Play();
                return; // don't allow accidentaly non-input
            }

            AnswerSubmitted = true;
            AllowSubmission = false;
            _answered = DateTime.Now;
        }

        private void HandleAnswerWasCorrect()
        {
            //_uiComponents.CanMessage.Show("Well done!");
            _finished = DateTime.Now;
            _model.RecordTest(_definitionForTesting, _posed, _answered, _finished, correct: true);
            Reset();
        }

        private void HandleAnswerWasIncorrect()
        {
            //_uiComponents.CanMessage.Show("Failure");
            _finished = DateTime.Now;
            _model.RecordTest(_definitionForTesting, _posed, _answered, _finished, correct: false);
            Reset();
        }

        private int untilModelReset = 5;
        private void Reset()
        {
            if (_isVocabTest)
            {
                Title = $"Remaining: {_vocabTestRemaining}";
                if (_vocabTestRemaining <= 0)
                {
                    Title = "Thank you";
                    _uiComponents.CanMessage.Show("Thank you for completing the initial assessment");
                    _parent.CloseThisAndOpenTestLaunchPad(this);
                }
                _vocabTestRemaining--;

                _definitionForTesting = _model.GetHighInformationDefinition();
            }
            else
            {
                _definitionForTesting = _model.GetHighLearningDefinition();
            }

            AnswerSubmitted = false;
            AllowSubmission = true;

            UserAnswer = "";

            Prompt        = _definitionForTesting.Prompt;
            CorrectAnswer = _definitionForTesting.CorrectAnswer;

            if (_definitionForTesting.SoundFileName is not null)
            {
                ShowPlayCurrentSoundButton = true;
                SoundPlayer player = new SoundPlayer(_definitionForTesting.SoundFileName);
                player.Load();

                SoundPlayer = player;

                player.Play();      
            } 
            else
            {
                SoundPlayer = null;
                ShowPlayCurrentSoundButton = false;
            }

            Contexts = _definitionForTesting.Contexts.Select(wic => AsRun(wic)).ToList();

            _posed = DateTime.Now;

            if (untilModelReset <= 0)
            {
                // TODO - maybe run this on a background thread
                // TODO - what if this gets slow??
                _model.UpdateVocabularyModel();
                untilModelReset = 5;
            }

            untilModelReset--;
        }

        private Tuple<string, string, string> AsRun(WordInContext wic)
        {
            string prepend;

            if (wic.WordStart == 0)
            {
                prepend = "";
            } else
            {
                prepend = wic.StatementText.Substring(0, wic.WordStart - 1);
            }
            
            string word    = wic.StatementText.Substring(wic.WordStart, wic.Len);

            int appendIndex = wic.WordStart + wic.Len + 1;
            string append;
            if (appendIndex >= wic.StatementText.Length)
            {
                append = "";
            } else
            {
                append = wic.StatementText.Substring(appendIndex);
            }
            // remove newlines since they make it look weird
            // TODO - what about song lyrics/subtitles etc
            // should we have a "meaningul newlines" flag??
            prepend = Regex.Replace(prepend, @"\t|\n|\r", " ");
            word    = Regex.Replace(word,    @"\t|\n|\r", " ");
            append  = Regex.Replace(append,  @"\t|\n|\r", " ");

            return Tuple.Create(prepend, word, append);
             
        }

    }
}

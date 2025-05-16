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
using QuickGraph;
using System.Windows.Annotations;
using System.Windows.Media.Animation;

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
        public ICommand CheckContextCommand        { get; private set; }
        public ICommand RepairDefinitionCommand    { get; private set; }
        public ICommand NextCommand                { get; private set; }
        public ICommand HideContextCommand         { get; private set; }

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

            CheckContextCommand     = new RelayCommand(() => CheckContext());
            RepairDefinitionCommand = new RelayCommand(() => RepairDefinition());
            NextCommand             = new RelayCommand(() => Reset());
            HideContextCommand      = new RelayCommand(() => HideSelectedContext());
            Reset();
        }

        private void HideSelectedContext()
        {
            if (_uiComponents.CanVerify.AskYesNo("Are you sure you want to hide this example from future testing?"))
            {
                WordInContext wic = _definitionForTesting.Contexts[_currentContextId];

                _model.HideStatementForTesting(wic.Parent);
            }
        }

        private void RepairDefinition()
        {
            if (!ShowNextButton)
            {
                // once we have paused already it will be shown, so if it isn't shown we need to tie up the loose ends
                // before going and trying to fix the definition
                // otherwise we will get weirdly long times for answering in the database

                bool correct = _uiComponents.CanVerify.AskYesNo("Before we leave, was your answer correct?");

                _finished = DateTime.Now;
                _model.RecordTest(_definitionForTesting, _posed, _answered, _finished, correct);

                Pause();
            }

            _parent.BeginDefinitionRepair(_definitionForTesting.Parent);
        }


        private void CheckContext()
        {
            if (!ShowNextButton)
            {
                // once we have paused already it will be shown, so if it isn't shown we need to tie up the loose ends
                // before going and trying to fix the definition
                // otherwise we will get weirdly long times for answering in the database

                bool correct;

                if (!AnswerSubmitted)
                {

                    UserAnswer = _uiComponents.CanGetText.GetResponse("Before we leave, what is your answer?");

                    while (DisallowSubmission())
                    {
                        UserAnswer = _uiComponents.CanGetText.GetResponse("Please provide an answer: ");
                    }

                    SubmitAnswer();

                    correct = _uiComponents.CanVerify.AskYesNo("Were you correct?");

                } else
                {
                    correct = _uiComponents.CanVerify.AskYesNo("Before we leave, was your answer correct?");
                }

                _finished = DateTime.Now;
                _model.RecordTest(_definitionForTesting, _posed, _answered, _finished, correct);

                Pause();
            }

            WordInContext wic = _definitionForTesting.Contexts[_currentContextId];

            _parent.BeginDefinitionResolution(wic.Parent, wic.Index);
        }

        private void Pause()
        {
            SubmissionButtonsEnabled = false;
            ShowNextButton           = true;
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
            _vocabTestRemaining = _model.VocabTestWordCount ?? throw new Exception("couldn't access definition learning service");

            _vocabTestRemaining -= _model.DistinctWordsTested;

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
        private bool _submissionButtonsEnabled = true;
        private bool _showNextButton = false;

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

        public bool SubmissionButtonsEnabled
        {
            get => _submissionButtonsEnabled;
            private set
            {
                _submissionButtonsEnabled = value;
                OnPropertyChanged(nameof(SubmissionButtonsEnabled));
            }
        }

        public bool ShowNextButton
        {
            get => _showNextButton;
            private set
            {
                _showNextButton = value;
                OnPropertyChanged(nameof(ShowNextButton));
            }
        }


        private bool DisallowSubmission()
        {
            return UserAnswer == "" || UserAnswer is null;
        }

        private void SubmitAnswer()
        {
            if (DisallowSubmission())
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

        private void Reset()
        {
            ShowNextButton           = false;
            SubmissionButtonsEnabled = true;

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
                try
                {
                    ShowPlayCurrentSoundButton = true;
                    SoundPlayer player = new SoundPlayer(_definitionForTesting.SoundFileName);
                    player.Load();

                    SoundPlayer = player;

                    player.Play();
                } catch (Exception e )
                {
                    _uiComponents.CanMessage.Show($"threw: {e.Message}\n was trying to load {_definitionForTesting.SoundFileName}");

                    // TODO - add a wizard to delete the entry if this happens

                    SoundPlayer = null;
                    ShowPlayCurrentSoundButton = false;
                }
            } 
            else
            {
                SoundPlayer = null;
                ShowPlayCurrentSoundButton = false;
            }

            Contexts = _definitionForTesting.Contexts.Select(wic => StatementHelper.AsRun(wic)).ToList();

            _posed = DateTime.Now;

        }
    }
}

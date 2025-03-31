using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using UserInputInterfaces;
using System.Windows;
using Windows.UI.Input;
using Infrastructure.DataClasses;

namespace Linguine.Tabs
{
    class TestLearningViewModel : TabViewModelBase
    {
        public ICommand SubmitAnswerCommand        { get; private set; }
        public ICommand SubmissionCorrectCommand   { get; private set; }
        public ICommand SubmissionIncorrectCommand { get; private set; }
        public ICommand SetFocusCommand            { get; private set; }


        private bool _isVocabTest       { get; set; }
        private int _vocabTestRemaining { get; set; }

        private DateTime _posed;
        private DateTime _answered;
        private DateTime _finished;

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
            Title = "Test";

            if (isVocabTest)
            {
                SetupVocabTest();
            }

            SubmitAnswerCommand        = new RelayCommand(() => SubmitAnswer());
            SubmissionCorrectCommand   = new RelayCommand(() => HandleAnswerWasCorrect());
            SubmissionIncorrectCommand = new RelayCommand(() => HandleAnswerWasIncorrect());
            SetFocusCommand            = new RelayCommand<object>(SetFocus);

            Reset();
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
                _definitionForTesting = _model.GetRandomDefinitionForTesting();
            }

            AnswerSubmitted = false;
            AllowSubmission = true;

            UserAnswer = "";

            Prompt        = _definitionForTesting.Prompt;
            CorrectAnswer = _definitionForTesting.CorrectAnswer;

            _posed = DateTime.Now;
        }

    }
}

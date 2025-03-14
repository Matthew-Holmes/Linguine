using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using UserInputInterfaces;
using System.Windows;

namespace Linguine.Tabs
{
    class TestLearningViewModel : TabViewModelBase
    {
        public ICommand SubmitAnswerCommand        { get; private set; }
        public ICommand SubmissionCorrectCommand   { get; private set; }
        public ICommand SubmissionIncorrectCommand { get; private set; }
        public ICommand SetFocusCommand            { get; private set; }

        private void SetFocus(object param)
        {
            if (param is Button button)
            {
                Application.Current.Dispatcher.InvokeAsync(() => button.Focus());
            }
        }

        public TestLearningViewModel(UIComponents  uiComponents, 
                                     MainModel     model, 
                                     MainViewModel parent) 
            : base(uiComponents, model, parent)
        {
            Title = "Test";

            SubmitAnswerCommand        = new RelayCommand(() => SubmitAnswer());
            SubmissionCorrectCommand   = new RelayCommand(() => HandleAnswerWasCorrect());
            SubmissionIncorrectCommand = new RelayCommand(() => HandleAnswerWasIncorrect());
            SetFocusCommand            = new RelayCommand<object>(SetFocus);

            _definitionForTesting = model.GetRandomDefinitionForTesting();

            Prompt = _definitionForTesting.prompt;
            CorrectAnswer = _definitionForTesting.correctAnswer;
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
        }

        private void HandleAnswerWasCorrect()
        {
            _uiComponents.CanMessage.Show("Well done!");
        }

        private void HandleAnswerWasIncorrect()
        {
            _uiComponents.CanMessage.Show("Failure");
        }

    }
}

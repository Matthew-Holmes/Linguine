using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UserInputInterfaces;

namespace Linguine.Tabs
{
    class TestLearningViewModel : TabViewModelBase
    {
        public ICommand SubmitAnswerCommand        { get; private set; }
        public ICommand SubmissionCorrectCommand   { get; private set; }
        public ICommand SubmissionIncorrectCommand { get; private set; }


        public TestLearningViewModel(UIComponents  uiComponents, 
                                     MainModel     model, 
                                     MainViewModel parent) 
            : base(uiComponents, model, parent)
        {
            Title = "Test";

            SubmitAnswerCommand        = new RelayCommand(() => SubmitAnswer());
            SubmissionCorrectCommand   = new RelayCommand(() => HandleAnswerWasCorrect());
            SubmissionIncorrectCommand = new RelayCommand(() => HandleAnswerWasIncorrect());
        }

        private String _prompt;
        private String _correctAnswer;
        private string usersAnswer;

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

        public String UsersAnswer
        {
            get => usersAnswer;
            set 
            { 
                usersAnswer = value;
                OnPropertyChanged(nameof(UsersAnswer));
            }
        }

        private void SubmitAnswer()
        {
            throw new NotImplementedException();
        }

        private void HandleAnswerWasCorrect()
        {
            throw new NotImplementedException();
        }

        private void HandleAnswerWasIncorrect()
        {
            throw new NotImplementedException();
        }

    }
}

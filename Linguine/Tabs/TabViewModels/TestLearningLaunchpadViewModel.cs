using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UserInputInterfaces;

namespace Linguine.Tabs 
{ 
    public class TestLearningLaunchpadViewModel : TabViewModelBase
    {
        private bool _enoughDataForWordFrequencies;

        public ICommand FreeStudyCommand     { get; private set; }
        public ICommand TargetedStudyCommand { get; private set; }

        public bool EnoughDataForWordFrequencies { get; private set; }
        public bool NeedToBurnInVocabularyData   { get; private set; }

        public TestLearningLaunchpadViewModel(UIComponents uiComponents, MainModel model, MainViewModel parent) : base(uiComponents, model, parent)
        {
            Title = "Test";

            FreeStudyCommand     = new RelayCommand(() => BeginFreeStudy());
            TargetedStudyCommand = new RelayCommand(() => BeginTargetedStudy());

            ValidateSufficentData();
        }

        private void ValidateSufficentData()
        {
            EnoughDataForWordFrequencies = _model.EnoughDataForWordFrequencies();
            NeedToBurnInVocabularyData = _model.NeedToBurnInVocabularyData();
        }

        private void BeginTargetedStudy()
        {
            _parent.CloseThisAndBeginTargetedStudy(this);
        }

        private void BeginFreeStudy()
        {
            _parent.CloseThisAndBeginFreeStudy(this);
        }
    }
}

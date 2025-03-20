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
        public ICommand FreeStudyCommand     { get; private set; }
        public ICommand TargetedStudyCommand { get; private set; }

        public bool EnoughDataForWordFrequencies { get; private set; }
        public bool AnyDataForWordFrequencies    { get; private set; }
        public bool NeedToBurnInVocabularyData   { get; private set; }

        public bool NeedADictionary { get; private set; }

        public bool TellUserToProcessMoreData => !EnoughDataForWordFrequencies && !NeedADictionary;
        public bool TellUserToDoVocabBurnin   => NeedToBurnInVocabularyData && EnoughDataForWordFrequencies && !NeedADictionary;
        public bool FreeStudyIsEnabled        => EnoughDataForWordFrequencies && !NeedToBurnInVocabularyData;
        public bool TargetedStudyEnabled      => AnyDataForWordFrequencies;

        public String NeedADictionaryText { get; } = "Please import a dictionary to begin learning";
        public String NeedMoreDataText    { get; } = "Not enough processed text for learning";
        public String NeedVocabBurnInText { get; } = "Please complete an initial vocabulary assessment";


        public TestLearningLaunchpadViewModel(UIComponents uiComponents, MainModel model, MainViewModel parent) : base(uiComponents, model, parent)
        {
            Title = "Test";

            FreeStudyCommand     = new RelayCommand(() => BeginFreeStudy());
            TargetedStudyCommand = new RelayCommand(() => BeginTargetedStudy());

            ValidateSufficentData();
        }

        private void ValidateSufficentData()
        {
            NeedADictionary              = _model.NeedToImportADictionary;
            EnoughDataForWordFrequencies = _model.EnoughDataForWordFrequencies();
            AnyDataForWordFrequencies    = _model.AnyDataForWordFrequencies();
            NeedToBurnInVocabularyData   = _model.NeedToBurnInVocabularyData();
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserInputInterfaces;
using DataClasses;
using Linguine.Helpers;
using System.Windows.Input;

namespace Linguine.Tabs
{
    class DefinitionRepairViewModel : TabViewModelBase
    {
        private string _definitionCoreText;
        private EditMethod _coredDefinitionChanged;

        private string _parsedDefinitionText;
        private EditMethod _parsedDefinitionChanged;

        private string _ipaText;
        private EditMethod _ipaChanged;

        private string _romanisedText;
        private EditMethod _romanisedChanged;

        public string DefinitionCoreText
        {
            get => _definitionCoreText;
            set
            {
                _definitionCoreText = value;
                OnPropertyChanged(nameof(DefinitionCoreText));
            }
        }

        public EditMethod CoredDefinitionChanged
        {
            get => _coredDefinitionChanged;
            set
            {
                _coredDefinitionChanged = value;
                OnPropertyChanged(nameof(CoredDefinitionChanged));
            }
        }

        public string ParsedDefinitionText
        {
            get => _parsedDefinitionText;
            set
            {
                _parsedDefinitionText = value;
                OnPropertyChanged(nameof(ParsedDefinitionText));
            }
        }

        public EditMethod ParsedDefinitionChanged
        {
            get => _parsedDefinitionChanged;
            set
            {
                _parsedDefinitionChanged = value;
                OnPropertyChanged(nameof(ParsedDefinitionChanged));
            }
        }

        public string IpaPronunciation
        {
            get => _ipaText;
            set
            {
                _ipaText = value;
                OnPropertyChanged(nameof(IpaPronunciation));
            }
        }

        public EditMethod IpaChanged
        {
            get => _ipaChanged;
            set
            {
                _ipaChanged = value;
                OnPropertyChanged(nameof(IpaChanged));
            }
        }

        public string RomanisedPronunciation
        {
            get => _romanisedText;
            set
            {
                _romanisedText = value;
                OnPropertyChanged(nameof(RomanisedPronunciation));
            }
        }

        public EditMethod RomanisedChanged
        {
            get => _romanisedChanged;
            set
            {
                _romanisedChanged = value;
                OnPropertyChanged(nameof(RomanisedChanged));
            }
        }

        public ICommand MachineRefreshCoreDefinitionCommand   { get; set; }
        public ICommand UserRefreshCoreDefinitionCommand      { get; set; }

        public ICommand MachineRefreshParsedDefinitionCommand { get; set; }
        public ICommand UserRefreshParsedDefinitionCommand    { get; set; }

        public ICommand MachineRefreshIpaCommand              { get; set; }
        public ICommand UserRefreshIpaCommand                 { get; set; }

        public ICommand MachineRefreshRomanisedCommand        { get; set; }
        public ICommand UserRefreshRomanisedCommand           { get; set; }

        public DefinitionRepairViewModel(DictionaryDefinition faulty, UIComponents uiComponents, MainModel model, MainViewModel parent)
            : base(uiComponents, model, parent)
        {
            Title = "Repair Definition";

            ParsedDictionaryDefinition? pdef = _model.GetParsedDictionaryDefinition(faulty);

            DefinitionCoreText     = faulty.Definition;
            ParsedDefinitionText   = pdef?.ParsedDefinition ?? "";
            IpaPronunciation       = faulty.IPAPronunciation ?? "";
            RomanisedPronunciation = faulty.RomanisedPronuncation ?? "";

            CoredDefinitionChanged  = EditMethod.NotEdited;
            ParsedDefinitionChanged = EditMethod.NotEdited;
            IpaChanged              = EditMethod.NotEdited;
            RomanisedChanged        = EditMethod.NotEdited;

            MachineRefreshCoreDefinitionCommand = new RelayCommand(() => RefreshCoreDefinitionFromMachine());
            UserRefreshCoreDefinitionCommand    = new RelayCommand(() => PromptUserCoreDefinition());

            MachineRefreshParsedDefinitionCommand = new RelayCommand(() => RefreshParsedDefinitionFromMachine());
            UserRefreshParsedDefinitionCommand    = new RelayCommand(() => PromptUserParsedDefinition());

            MachineRefreshIpaCommand = new RelayCommand(() => RefreshIpaFromMachine());
            UserRefreshIpaCommand    = new RelayCommand(() => PromptUserIpa());

            MachineRefreshRomanisedCommand = new RelayCommand(() => RefreshRomanisedFromMachine());
            UserRefreshRomanisedCommand    = new RelayCommand(() => PromptUserRomanised());
        }

        private void RefreshCoreDefinitionFromMachine()
        {
            DefinitionCoreText = GenerateCoreDefinition();
            CoredDefinitionChanged = EditMethod.MachineEdited;
        }

        private void PromptUserCoreDefinition()
        {
            DefinitionCoreText = PromptForUserInput("Edit Core Definition");
            CoredDefinitionChanged = EditMethod.UserEdited;
        }

        private void RefreshParsedDefinitionFromMachine()
        {
            ParsedDefinitionText = GenerateParsedDefinition();
            ParsedDefinitionChanged = EditMethod.MachineEdited;
        }

        private void PromptUserParsedDefinition()
        {
            ParsedDefinitionText = PromptForUserInput("Edit Parsed Definition");
            ParsedDefinitionChanged = EditMethod.UserEdited;
        }

        private void RefreshIpaFromMachine()
        {
            IpaPronunciation = GenerateIpa();
            IpaChanged = EditMethod.MachineEdited;
        }

        private void PromptUserIpa()
        {
            IpaPronunciation = PromptForUserInput("Edit IPA");
            IpaChanged = EditMethod.UserEdited;
        }

        private void RefreshRomanisedFromMachine()
        {
            RomanisedPronunciation = GenerateRomanised();
            RomanisedChanged = EditMethod.MachineEdited;
        }

        private void PromptUserRomanised()
        {
            RomanisedPronunciation = PromptForUserInput("Edit Romanised");
            RomanisedChanged = EditMethod.UserEdited;
        }

        // Placeholder business logic methods
        private string GenerateCoreDefinition() => throw new NotImplementedException();
        private string GenerateParsedDefinition() => throw new NotImplementedException();
        private string GenerateIpa() => throw new NotImplementedException();
        private string GenerateRomanised() => throw new NotImplementedException();
        private string PromptForUserInput(string fieldName) => throw new NotImplementedException();
    }

}

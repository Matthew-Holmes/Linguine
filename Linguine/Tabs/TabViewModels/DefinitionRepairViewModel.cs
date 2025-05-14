using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserInputInterfaces;
using DataClasses;
using Linguine.Helpers;
using System.Windows.Input;
using Config;

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

        #region UI properties

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

        public bool ButtonsEnabled
        {
            get => _buttonsEnabled;
            set
            {
                _buttonsEnabled = value;
                OnPropertyChanged(nameof(ButtonsEnabled));
            }
        }

        public bool ShowParsing { get; init; }

        #endregion

        #region commands

        public ICommand MachineRefreshCoreDefinitionCommand   { get; set; }
        public ICommand UserRefreshCoreDefinitionCommand      { get; set; }

        public ICommand MachineRefreshParsedDefinitionCommand { get; set; }
        public ICommand UserRefreshParsedDefinitionCommand    { get; set; }

        public ICommand MachineRefreshIpaCommand              { get; set; }
        public ICommand UserRefreshIpaCommand                 { get; set; }

        public ICommand MachineRefreshRomanisedCommand        { get; set; }
        public ICommand UserRefreshRomanisedCommand           { get; set; }

        #endregion

        DictionaryDefinition faulty;
        private bool _buttonsEnabled = true;

        public DefinitionRepairViewModel(DictionaryDefinition faultyDef, UIComponents uiComponents, MainModel model, MainViewModel parent)
            : base(uiComponents, model, parent)
        {
            Title = "Repair Definition";

            faulty = faultyDef;

            ParsedDictionaryDefinition? pdef = _model.GetParsedDictionaryDefinition(faulty);

            DefinitionCoreText     = faulty.Definition;
            ParsedDefinitionText   = pdef?.ParsedDefinition ?? "";
            IpaPronunciation       = faulty.IPAPronunciation ?? "";
            RomanisedPronunciation = faulty.RomanisedPronuncation ?? "";

            CoredDefinitionChanged  = EditMethod.NotEdited;
            ParsedDefinitionChanged = EditMethod.NotEdited;
            IpaChanged              = EditMethod.NotEdited;
            RomanisedChanged        = EditMethod.NotEdited;

            MachineRefreshCoreDefinitionCommand = new RelayCommand(() => Task.Run(MachineRefreshCoreDefinition));
            UserRefreshCoreDefinitionCommand    = new RelayCommand(() => PromptUserCoreDefinition());

            MachineRefreshParsedDefinitionCommand = new RelayCommand(() => MachineRefreshParsedDefinition());
            UserRefreshParsedDefinitionCommand    = new RelayCommand(() => PromptUserParsedDefinition());

            MachineRefreshIpaCommand = new RelayCommand(() => RefreshIpaFromMachine());
            UserRefreshIpaCommand    = new RelayCommand(() => PromptUserIpa());

            MachineRefreshRomanisedCommand = new RelayCommand(() => RefreshRomanisedFromMachine());
            UserRefreshRomanisedCommand    = new RelayCommand(() => PromptUserRomanised());

            ShowParsing = ConfigManager.Config.LearningForeignLanguage();
        }

        private async Task MachineRefreshCoreDefinition()
        {
            ButtonsEnabled = false;

            DefinitionCoreText = await _model.GenerateNewDefinition(faulty);
            CoredDefinitionChanged = EditMethod.MachineEdited;

            await MachineRefreshParsedDefinition();

            ButtonsEnabled = true;
        }

        private void PromptUserCoreDefinition()
        {
            String newDef = _uiComponents.CanGetText.GetResponse("enter the custom definition");

            if (String.IsNullOrWhiteSpace(newDef)) { return; }

            if (!_uiComponents.CanVerify.AskYesNo($"proceed with new definition: {newDef}"))
            {
                return;
            }

            DefinitionCoreText = newDef;
            CoredDefinitionChanged = EditMethod.UserEdited;
        }

        private ParsedDictionaryDefinition ParsedDefinition { get; set; }

        private async Task MachineRefreshParsedDefinition()
        {
            ButtonsEnabled = false;

            DictionaryDefinition custom = faulty; // in case we have changed the core text

            custom.Definition = DefinitionCoreText;

            ParsedDefinition = await _model.GenerateSingleParsedDefinition(custom);
            ParsedDefinitionText = ParsedDefinition.ParsedDefinition;

            ParsedDefinitionChanged = EditMethod.MachineEdited;

            ButtonsEnabled = true;
        }

        private void PromptUserParsedDefinition()
        {
            String newDef = _uiComponents.CanGetText.GetResponse("enter the custom definition");

            if (String.IsNullOrWhiteSpace(newDef)) { return; }

            if (!_uiComponents.CanVerify.AskYesNo($"proceed with new definition: {newDef}"))
            {
                return;
            }

            ParsedDefinitionText = newDef;
            ParsedDefinitionChanged = EditMethod.UserEdited;
        }

        private void RefreshIpaFromMachine()
        {
            IpaPronunciation = GenerateIpa();
            IpaChanged = EditMethod.MachineEdited;
        }

        private void PromptUserIpa()
        {
            String newIPA = _uiComponents.CanGetText.GetResponse("enter the custom IPA");

            if (String.IsNullOrWhiteSpace(newIPA)) { return; }

            if (!_uiComponents.CanVerify.AskYesNo($"proceed with new IPA: {newIPA}"))
            {
                return;
            }

            IpaPronunciation = newIPA;
            IpaChanged = EditMethod.UserEdited;
        }

        private void RefreshRomanisedFromMachine()
        {
            RomanisedPronunciation = GenerateRomanised();
            RomanisedChanged = EditMethod.MachineEdited;
        }

        private void PromptUserRomanised()
        {
            String newRomanised = _uiComponents.CanGetText.GetResponse("enter the custom romanised pronunciation");

            if (String.IsNullOrWhiteSpace(newRomanised)) { return; }

            if (!_uiComponents.CanVerify.AskYesNo($"proceed with new romanised pronunciation: {newRomanised}"))
            {
                return;
            }

            RomanisedPronunciation = newRomanised;
            RomanisedChanged = EditMethod.UserEdited;
        }

        // Placeholder business logic methods
        private string GenerateParsedDefinition() => throw new NotImplementedException();
        private string GenerateIpa() => throw new NotImplementedException();
        private string GenerateRomanised() => throw new NotImplementedException();
    }

}

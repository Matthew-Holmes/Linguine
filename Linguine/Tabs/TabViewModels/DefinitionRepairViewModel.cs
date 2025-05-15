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
        private bool _showCoreDefinitionSaveChanges = false;

        private string _parsedDefinitionText;
        private EditMethod _parsedDefinitionChanged;
        private bool _showParsedDefinitionSaveChanges = false;

        private string _ipaText;
        private EditMethod _ipaChanged;
        private bool _showIPASaveChanges = false;

        private string _romanisedText;
        private EditMethod _romanisedChanged;
        private bool _showRomanisedSaveChanges = false;

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

        public bool ShowCoreDefinitionSaveChanges
        {
            get => _showCoreDefinitionSaveChanges;
            set
            {
                _showCoreDefinitionSaveChanges = value;
                OnPropertyChanged(nameof(ShowCoreDefinitionSaveChanges));
            }
        }

        public bool ShowParsedDefinitionSaveChanges
        {
            get => _showParsedDefinitionSaveChanges;
            set
            {
                _showParsedDefinitionSaveChanges = value;
                OnPropertyChanged(nameof(ShowParsedDefinitionSaveChanges));
            }
        }

        public bool ShowIPASaveChanges
        {
            get => _showIPASaveChanges;
            set
            {
                _showIPASaveChanges = value;
                OnPropertyChanged(nameof(ShowIPASaveChanges));
            }
        }

        public bool ShowRomanisedSaveChanges
        {
            get => _showRomanisedSaveChanges;
            set
            {
                _showRomanisedSaveChanges = value;
                OnPropertyChanged(nameof(ShowRomanisedSaveChanges));
            }
        }

        public bool ShowParsing { get; init; }

        #endregion

        #region commands

        public ICommand MachineRefreshCoreDefinitionCommand   { get; set; }
        public ICommand UserRefreshCoreDefinitionCommand      { get; set; }
        public ICommand SaveCoreDefinitionChangesCommand      { get; set; }

        public ICommand MachineRefreshParsedDefinitionCommand { get; set; }
        public ICommand UserRefreshParsedDefinitionCommand    { get; set; }
        public ICommand SaveParsedDefinitionChangesCommand    { get; set; }

        public ICommand MachineRefreshIpaCommand              { get; set; }
        public ICommand UserRefreshIpaCommand                 { get; set; }
        public ICommand SaveIPAChangesCommand                 { get; set; }

        public ICommand MachineRefreshRomanisedCommand        { get; set; }
        public ICommand UserRefreshRomanisedCommand           { get; set; }
        public ICommand SaveRomanisedChangesCommand           { get; set; }


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
            UserRefreshCoreDefinitionCommand    = new RelayCommand(() => Task.Run(PromptUserCoreDefinition));
            SaveCoreDefinitionChangesCommand    = new RelayCommand(() => SaveCoreDefinitionChanges());

            MachineRefreshParsedDefinitionCommand = new RelayCommand(() => Task.Run(MachineRefreshParsedDefinition));
            UserRefreshParsedDefinitionCommand    = new RelayCommand(() => PromptUserParsedDefinition());
            SaveParsedDefinitionChangesCommand    = new RelayCommand(() => SaveParsedDefinitionChanges());

            MachineRefreshIpaCommand = new RelayCommand(() => Task.Run(MachineRefreshIpa));
            UserRefreshIpaCommand    = new RelayCommand(() => PromptUserIpa());
            SaveIPAChangesCommand    = new RelayCommand(() => SaveIPAChanges());

            MachineRefreshRomanisedCommand = new RelayCommand(() => Task.Run(MachineRefreshRomanised));
            UserRefreshRomanisedCommand    = new RelayCommand(() => PromptUserRomanised());
            SaveRomanisedChangesCommand    = new RelayCommand(() => SaveRomanisedChanges());

            ShowParsing = ConfigManager.Config.LearningForeignLanguage();
        }

        private void SaveRomanisedChanges()
        {
            _model.UpdateRomanised(faulty, RomanisedPronunciation, RomanisedChanged);
            RomanisedChanged = EditMethod.NotEdited; // since now the default
        }

        private void SaveIPAChanges()
        {
            _model.UpdateIPA(faulty, IpaPronunciation, IpaChanged);
            IpaChanged = EditMethod.NotEdited;
        }

        private void SaveParsedDefinitionChanges()
        {
            _model.UpdateParsedDefinition(ParsedDefinition, ParsedDefinitionText, ParsedDefinitionChanged);
            ParsedDefinitionChanged = EditMethod.NotEdited;
        }

        private void SaveCoreDefinitionChanges()
        {
            _model.UpdateCoreDefinition(faulty, DefinitionCoreText, CoredDefinitionChanged);
            CoredDefinitionChanged = EditMethod.NotEdited;
        }

        private async Task MachineRefreshCoreDefinition()
        {
            ButtonsEnabled = false;

            String newDef = await _model.GenerateNewDefinition(faulty);

            if (newDef != DefinitionCoreText)
            {
                DefinitionCoreText = newDef;
                CoredDefinitionChanged = EditMethod.MachineEdited;
                await MachineRefreshParsedDefinition();

                ShowCoreDefinitionSaveChanges = true;
            } 
            else
            {
                _uiComponents.CanMessage.Show("machine generated the same as before!");
            }

            ButtonsEnabled = true;
        }

        private async Task PromptUserCoreDefinition()
        {
            String newDef = _uiComponents.CanGetText.GetResponse("enter the custom definition");

            if (String.IsNullOrWhiteSpace(newDef)) { return; }

            if (!_uiComponents.CanVerify.AskYesNo($"proceed with new definition: {newDef}"))
            {
                return;
            }

            if (newDef != DefinitionCoreText)
            {
                DefinitionCoreText = newDef;
                CoredDefinitionChanged = EditMethod.UserEdited;
                await MachineRefreshParsedDefinition();
                ShowCoreDefinitionSaveChanges = true;
            } 
            else
            {
                _uiComponents.CanMessage.Show("you provided the existing definition!");
            }
        }

        private ParsedDictionaryDefinition ParsedDefinition { get; set; }

        private async Task MachineRefreshParsedDefinition()
        {
            ButtonsEnabled = false;

            DictionaryDefinition custom = faulty; // in case we have changed the core text

            custom.Definition = DefinitionCoreText;

            ParsedDictionaryDefinition newPdef = await _model.GenerateSingleParsedDefinition(custom);

            if (newPdef.ParsedDefinition != ParsedDefinitionText)
            {
                ParsedDefinition = newPdef;
                ParsedDefinitionText = ParsedDefinition.ParsedDefinition;

                ShowParsedDefinitionSaveChanges = true;
            }
            else
            {
                _uiComponents.CanMessage.Show("Machine generated the same parsed definition!");
            }

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

            if (newDef != ParsedDefinitionText)
            {
                ParsedDefinitionText = newDef;
                ParsedDefinitionChanged = EditMethod.UserEdited;
                ShowParsedDefinitionSaveChanges = true;
            } 
            else
            {
                _uiComponents.CanMessage.Show("You provided the existing parsed definition!");
            }
        }

        private async Task MachineRefreshIpa()
        {
            ButtonsEnabled = false;

            String newIpa = await _model.GetNewIPA(faulty);

            if (newIpa != IpaPronunciation)
            {
                IpaPronunciation = newIpa;
                IpaChanged = EditMethod.MachineEdited;

                ShowIPASaveChanges = true;
            } 
            else
            {
                _uiComponents.CanMessage.Show("Machine generated the same IPA as before");
            }

            ButtonsEnabled = true;
        }

        private void PromptUserIpa()
        {
            String newIPA = _uiComponents.CanGetText.GetResponse("enter the custom IPA");

            if (String.IsNullOrWhiteSpace(newIPA)) { return; }

            if (!_uiComponents.CanVerify.AskYesNo($"proceed with new IPA: {newIPA}"))
            {
                return;
            }

            if (newIPA != IpaPronunciation)
            {
                IpaPronunciation = newIPA;
                IpaChanged = EditMethod.UserEdited;

                ShowIPASaveChanges = true;
            } 
            else
            {
                _uiComponents.CanMessage.Show("You provided the existing IPA");
            }
        }

        private async Task MachineRefreshRomanised()
        {
            ButtonsEnabled = false;

            String newRoman = await _model.GetNewRomanised(faulty);

            if (newRoman != RomanisedPronunciation)
            {
                RomanisedPronunciation = newRoman;
                RomanisedChanged = EditMethod.MachineEdited;

                ShowRomanisedSaveChanges = true;
            } else
            {
                _uiComponents.CanMessage.Show("Machine generated the same Romanised pronunciation");
            }

            ButtonsEnabled = true;
        }

        private void PromptUserRomanised()
        {
            String newRomanised = _uiComponents.CanGetText.GetResponse("enter the custom romanised pronunciation");

            if (String.IsNullOrWhiteSpace(newRomanised)) { return; }

            if (!_uiComponents.CanVerify.AskYesNo($"proceed with new romanised pronunciation: {newRomanised}"))
            {
                return;
            }

            if (newRomanised != RomanisedPronunciation)
            {

                RomanisedPronunciation = newRomanised;
                RomanisedChanged = EditMethod.UserEdited;

                ShowRomanisedSaveChanges = true;
            } else
            {
                _uiComponents.CanMessage.Show("You provided the existing pronunciation");
            }
        }
    }
}

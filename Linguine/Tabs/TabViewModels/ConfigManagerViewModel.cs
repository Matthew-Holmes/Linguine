using Infrastructure;
using System;
using System.Collections.Generic;
using DataClasses;
using Config;
using System.Windows.Input;
using UserInputInterfaces;
using Microsoft.VisualBasic.Logging;

namespace Linguine.Tabs
{
    public class ConfigManagerViewModel : TabViewModelBase
    {
        #region language selection
        private List<LanguageCode> _languageCodes = LanguageCodeDetails.LanguageCodes();
        private List<LearnerLevel> _learnerLevels = LearnerLevelDetails.LearnerLevels();

        private int _targetLanguageIndex;
        private int _nativeLanguageIndex;
        private int _learnerLevelIndex;

        private List<String> _languageOptions;
        public  List<String> LanguageOptions { get => _languageOptions; private set => _languageOptions = value; }

        private List<String> _learnerLevelOptions;
        private bool _addDictionaryEnabled;

        public List<String> LearnerLevelOptions { 
            get => _learnerLevelOptions; private set => _learnerLevelOptions = value; }

        public String TargetLanguage => LanguageOptions[TargetLanguageIndex];
        public String NativeLanguage => LanguageOptions[NativeLanguageIndex];
        public String LearnerLevel => LearnerLevelOptions[LearnerLevelIndex];
        public int TargetLanguageIndex
        {
            get => _targetLanguageIndex;
            set
            {
                if (value == -1 || value == _targetLanguageIndex)
                {
                    // wpf likes trying to set this to -1 when the options are updating
                    return;
                }

                Config.Config newconfig = ConfigManager.Config with
                {
                    Languages = ConfigManager.Config.Languages with
                    {
                        TargetLanguage = _languageCodes[value]
                    }
                };

                ConfigManager.SaveConfig(newconfig);

                _targetLanguageIndex = value;

                OnPropertyChanged(nameof(LearnerLevel));
                OnPropertyChanged(nameof(TargetLanguage));
            }
        }

        public int NativeLanguageIndex
        {
            get => _nativeLanguageIndex;
            set
            {
                if (value == -1)
                {
                    return;
                }

                UpdateNativeLanguageInConfig(_languageCodes[value]);
                _nativeLanguageIndex = value;

                OnPropertyChanged(nameof(LanguageOptions));
                OnPropertyChanged(nameof(NativeLanguage));
                OnPropertyChanged(nameof(TargetLanguage));
            }
        }

        public int LearnerLevelIndex
        {
            get => _learnerLevels.IndexOf(ConfigManager.Config.GetLearnerLevel());
            set
            {
                if (value == -1)
                {
                    return;
                }

                Config.Config newConfig = ConfigManager.WithNewLearnerLevel(_learnerLevels[value]);
                ConfigManager.SaveConfig(newConfig);

                _learnerLevelIndex = value;

                OnPropertyChanged(nameof(LearnerLevel));
            }
        }

        private void UpdateNativeLanguageInConfig(LanguageCode newNative)
        {
            Config.Config config = ConfigManager.Config;

            if (newNative != config.Languages.NativeLanguage)
            {
                Config.Config newconfig = config with
                {
                    Languages = config.Languages with
                    {
                        NativeLanguage = newNative
                    }
                };

                ConfigManager.SaveConfig(newconfig);

                LanguageOptions = LanguageCodeDetails.LanguageNames(newNative);
            }
        }

        private void SetupLanguageSelection()
        {
            Config.Config config = ConfigManager.Config;

            _languageOptions     = LanguageCodeDetails.LanguageNames(config.Languages.NativeLanguage);
            _learnerLevelOptions = LearnerLevelDetails.LearnerLevelNames();

            _nativeLanguageIndex = _languageCodes.IndexOf(config.Languages.NativeLanguage);
            _targetLanguageIndex = _languageCodes.IndexOf(config.Languages.TargetLanguage);
            _learnerLevelIndex   = _learnerLevels.IndexOf(config.GetLearnerLevel());
        }
        #endregion

        #region dictionary loading

        public bool AddDictionaryEnabled
        {
            get => _addDictionaryEnabled;
            set
            {
                _addDictionaryEnabled = value;
                OnPropertyChanged(nameof(AddDictionaryEnabled));
            }
        }
        public ICommand AddTargetDictionaryCommand { get; private set; }

        private void SetupDictionaryImporting()
        {
            // TODO - some sort of state checking

            if (_parent.Model is not null)
            {
                if (_parent.Model.SM.ManagerState == DataManagersState.Initialised) 
                { 
                    RefreshAddDictionaryEnabled(); 
                }
            }

            _model.Loaded += (s,e) => RefreshAddDictionaryEnabled();

            AddTargetDictionaryCommand = new RelayCommand(() => AddTargetDictionary());
        }

        private void AddTargetDictionary()
        {
            if (_uiComponents.CanVerify.AskYesNo($"add a {TargetLanguage} dictionary for target language definitions?"))
            {
                AddNewDictionary();
            }
        }

        private void AddNewDictionary()
        {

            // TODO - check if a dictionary already exists

            String filename;

            while (true)
            {
                filename = _uiComponents.CanBrowseFiles.Browse();
                if (filename is not null && filename != "") { break; }
                if (_uiComponents.CanVerify.AskYesNo("Abort?")) { return; }
            }

            if (_model.SM.ManagerState == DataManagersState.Initialising)
            {
                _uiComponents.CanMessage.Show("Please wait for dictionary management to load");
                return;
            } else if (_model.SM.ManagerState == DataManagersState.NoDatabaseYet)
            {
                _uiComponents.CanMessage.Show("something went wrong! no database detected");
                return;
            }
            else if (_model.SM.ManagerState == DataManagersState.Initialised)
            {
                try
                {
                    using var context = _model.LinguineFactory.CreateDbContext(); // TODO - push this down to model method?

                    String? msg = _model.SM.Managers!.Definitions.AddDictionaryFromCSV(
                        filename, context, 
                        _model.SM.InspectDataQuality);

                    if (msg is not null)
                    {
                        _uiComponents.CanMessage.Show(msg);
                    }
                }
                catch (Exception e)
                {
                    _uiComponents.CanMessage.Show(e.Message);
                }

            } else
            {
                throw new NotImplementedException();
            }

            RefreshAddDictionaryEnabled();
        }

        private void RefreshAddDictionaryEnabled()
        {
            if (_model.SM.DataQuality == DataQuality.NeedDictionary)
            {
                AddDictionaryEnabled = true;
            }
            else
            {
                AddDictionaryEnabled = false;
            }
        }

        #endregion

        #region variants loading - disabled for now
        //public ICommand AddNewTargetVariantsCommand { get; private set; }

        //private void SetupVariantsSelection()
        //{
        //    RefreshVariantsAvailability();
        //    _model.Loaded += (s, e) => RefreshVariantsAvailability();

        //    AddNewTargetVariantsCommand = new RelayCommand(() => AddTargetVariants());
        //}

        //private void AddTargetVariants()
        //{
        //    if (_uiComponents.CanVerify.AskYesNo($"add a new {TargetLanguage} variants source for variant/root mapping?"))
        //    {
        //        AddVariants();
        //    }
        //}

        //private void AddVariants()
        //{
        //    String name;
        //    String filename;

        //    while (true)
        //    {
        //        filename = _uiComponents.CanBrowseFiles.Browse();
        //        if (filename is not null && filename != "") { break; }
        //        if (_uiComponents.CanVerify.AskYesNo("Abort?")) { return; }
        //    }

        //    while (true)
        //    {
        //        name = _uiComponents.CanGetText.GetResponse("Choose a name for the Dictionary");
        //        if (name is not null && name != "") { break; }
        //        if (_uiComponents.CanVerify.AskYesNo("Abort?")) { return; }
        //    }


        //    if (!_model.HasManagers)
        //    {
        //        _uiComponents.CanMessage.Show("please wait for variants manager to load");
        //    }
        //    else
        //    {
        //        VariantsManager manager = _model.VariantsManager;
        //        try
        //        {
        //            manager.AddNewVariantsSourceFromCSV(filename);
        //        }
        //        catch (Exception e)
        //        {
        //            _uiComponents.CanMessage.Show(e.Message);
        //        }
        //    }
        //    RefreshVariantsAvailability();
        //}

        //private void RefreshVariantsAvailability()
        //{
        //    throw new NotImplementedException();
        //}
        #endregion

        public ConfigManagerViewModel(UIComponents uiComponents, MainModel model, MainViewModel parent) 
            : base(uiComponents, model, parent)
        {
            Title = "Config Manager";
            SetupLanguageSelection();
            SetupDictionaryImporting();
            //SetupVariantsSelection();
        }
    }
}

using Infrastructure;
using Linguine.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using UserInputInterfaces;

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

                Config newconfig = ConfigManager.Config with
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

                Config newConfig = ConfigManager.WithNewLearnerLevel(_learnerLevels[value]);
                ConfigManager.SaveConfig(newConfig);

                _learnerLevelIndex = value;

                OnPropertyChanged(nameof(LearnerLevel));
            }
        }

        private void UpdateNativeLanguageInConfig(LanguageCode newNative)
        {
            Config config = ConfigManager.Config;

            if (newNative != config.Languages.NativeLanguage)
            {
                Config newconfig = config with
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
            Config config = ConfigManager.Config;

            _languageOptions     = LanguageCodeDetails.LanguageNames(config.Languages.NativeLanguage);
            _learnerLevelOptions = LearnerLevelDetails.LearnerLevelNames();

            _nativeLanguageIndex = _languageCodes.IndexOf(config.Languages.NativeLanguage);
            _targetLanguageIndex = _languageCodes.IndexOf(config.Languages.TargetLanguage);
            _learnerLevelIndex   = _learnerLevels.IndexOf(config.GetLearnerLevel());
        }
        #endregion

        #region dictionary loading
        public List<String> TargetLanguageDictionaries { get; private set; }

        public ICommand AddNewTargetDictionaryCommand { get; private set; }

        private void SetupDictionarySelection()
        {
            if (_parent.Model.HasManagers) { RefreshAvailableDictionaries(); }

            _model.Loaded += (s,e) => RefreshAvailableDictionaries();

            AddNewTargetDictionaryCommand = new RelayCommand(() => AddNewTargetDictionary());
        }

        private void AddNewTargetDictionary()
        {
            if (_uiComponents.CanVerify.AskYesNo($"add a new {TargetLanguage} dictionary for target language definitions?"))
            {
                AddNewDictionary();
            }
        }

        private void AddNewDictionary()
        {
            String name;
            String filename;

            while (true)
            {
                filename = _uiComponents.CanBrowseFiles.Browse();
                if (filename is not null && filename != "") { break; }
                if (_uiComponents.CanVerify.AskYesNo("Abort?")) { return; }
            }

            while (true)
            {
                name = _uiComponents.CanGetText.GetResponse("Choose a name for the Dictionary");
                if (name is not null && name != "") { break; }
                if (_uiComponents.CanVerify.AskYesNo("Abort?")) { return; }
            }

            if (!_model.HasManagers)
            {
                _uiComponents.CanMessage.Show("Please wait for dictionary management to load");
                return;
            }
            else
            {
                ExternalDictionaryManager manager = _model.ExternalDictionaryManager;
                try
                {
                    manager.AddNewDictionaryFromCSV(filename, name);
                }
                catch (Exception e)
                {
                    _uiComponents.CanMessage.Show(e.Message);
                }
            }

            RefreshAvailableDictionaries();
        }

        private void RefreshAvailableDictionaries()
        {
            TargetLanguageDictionaries = _model.ExternalDictionaryManager.AvailableDictionaries();

            OnPropertyChanged(nameof(TargetLanguageDictionaries));
        }

        #endregion

        #region variants loading
        public List<String> TargetLanguageVariantSources { get; private set; }
        public ICommand AddNewTargetVariantsSourceCommand { get; private set; }

        private void SetupVariantsSelection()
        {
            RefreshAvailableVariantsSources();
            _model.Loaded += (s, e) => RefreshAvailableVariantsSources();

            AddNewTargetVariantsSourceCommand = new RelayCommand(() => AddNewTargetVariantsSource());
        }

        private void AddNewTargetVariantsSource()
        {
            if (_uiComponents.CanVerify.AskYesNo($"add a new {TargetLanguage} variants source for variant/root mapping?"))
            {
                AddNewVariantsSource();
            }
        }

        private void AddNewVariantsSource()
        {
            String name;
            String filename;

            while (true)
            {
                filename = _uiComponents.CanBrowseFiles.Browse();
                if (filename is not null && filename != "") { break; }
                if (_uiComponents.CanVerify.AskYesNo("Abort?")) { return; }
            }

            while (true)
            {
                name = _uiComponents.CanGetText.GetResponse("Choose a name for the Dictionary");
                if (name is not null && name != "") { break; }
                if (_uiComponents.CanVerify.AskYesNo("Abort?")) { return; }
            }


            if (!_model.HasManagers)
            {
                _uiComponents.CanMessage.Show("please wait for variants manager to load");
            }
            else
            {
                VariantsManager manager = _model.VariantsManager;
                try
                {
                    manager.AddNewVariantsSourceFromCSV(filename, name);
                }
                catch (Exception e)
                {
                    _uiComponents.CanMessage.Show(e.Message);
                }
            }
            RefreshAvailableVariantsSources();
        }

        private void RefreshAvailableVariantsSources()
        {
            TargetLanguageVariantSources = _model.VariantsManager.AvailableVariantsSources();
            OnPropertyChanged(nameof(TargetLanguageVariantSources));
        }
        #endregion

        public ConfigManagerViewModel(UIComponents uiComponents, MainModel model, MainViewModel parent) 
            : base(uiComponents, model, parent)
        {
            Title = "Config Manager";
            SetupLanguageSelection();
            SetupDictionarySelection();
            SetupVariantsSelection();
        }

       
    }
}

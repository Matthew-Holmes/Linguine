using Infrastructure;
using LearningStore;
using Linguine.Helpers;
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

        private int _targetLanguageIndex;
        private int _nativeLanguageIndex;

        private List<String> _languageOptions;
        public  List<String> LanguageOptions { get => _languageOptions; private set => _languageOptions = value; }

        public String TargetLanguage => LanguageOptions[TargetLanguageIndex];
        public String NativeLanguage => LanguageOptions[NativeLanguageIndex];

        public int TargetLanguageIndex
        {
            get => _targetLanguageIndex;
            set
            {
                if (value == -1)
                {
                    // wpf likes trying to set this to -1 when the options are updating
                    return;
                }

                UpdateTargetLanguageInConfig(_languageCodes[value]);
                _targetLanguageIndex = value;

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

        private void UpdateTargetLanguageInConfig(LanguageCode newTarget)
        {
            if (newTarget != ConfigManager.TargetLanguage)
            {
                ConfigManager.TargetLanguage = newTarget;
            }
        }
        private void UpdateNativeLanguageInConfig(LanguageCode newNative)
        {
            if (newNative != ConfigManager.NativeLanguage)
            {
                ConfigManager.NativeLanguage = newNative;

                LanguageOptions = LanguageCodeDetails.LanguageNames(newNative);
            }
        }

        private void SetupLanguageSelection()
        {
            _languageOptions = LanguageCodeDetails.LanguageNames(ConfigManager.NativeLanguage);

            _nativeLanguageIndex = _languageCodes.IndexOf(ConfigManager.NativeLanguage);
            _targetLanguageIndex = _languageCodes.IndexOf(ConfigManager.TargetLanguage);
        }
        #endregion

        public List<String> TargetLanguageDictionaries { get; private set; }
        public List<String> NativeLanguageDictionaries { get; private set; }


        public ConfigManagerViewModel(UIComponents uiComponents, MainModel parent) : base(uiComponents, parent)
        {
            Title = "Config Manager";
            SetupLanguageSelection();
            SetupDictionarySelection();
        }

        private void SetupDictionarySelection()
        {
            RefreshAvailableDictionaries();
            ConfigFileHandler.ConfigUpdated += RefreshAvailableDictionaries;

            AddNewNativeDictionaryCommand = new RelayCommand(() => AddNewNativeDictionary());
            AddNewTargetDictionaryCommand = new RelayCommand(() => AddNewTargetDictionary());
        }

        private void AddNewTargetDictionary()
        {
            if (_uiComponents.CanVerify.AskYesNo($"add a new {TargetLanguage} dictionary for target language definitions?"))
            {
                AddNewDictionary(ConfigManager.TargetLanguage);
            }
        }

        private void AddNewNativeDictionary()
        {
            if (_uiComponents.CanVerify.AskYesNo($"add a new {NativeLanguage} dictionary for native language definitions?"))
            {
                AddNewDictionary(ConfigManager.NativeLanguage);
            }
        }

        private void AddNewDictionary(LanguageCode lc)
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
                name = _uiComponents.CanGetText.GetResponse("Choose a name for the Dictioary");
                if (name is not null && name != "") { break; }
                if (_uiComponents.CanVerify.AskYesNo("Abort?")) { return; }
            }

            try
            {
                ExternalDictionaryManager.AddNewDictionaryFromCSV(lc, name, filename);
            } catch (Exception e)
            {
                _uiComponents.CanMessage.Show(e.Message);
            }

            RefreshAvailableDictionaries();
        }

        private void RefreshAvailableDictionaries()
        {
            TargetLanguageDictionaries = ExternalDictionaryManager.AvailableDictionaries(ConfigManager.TargetLanguage);
            NativeLanguageDictionaries = ExternalDictionaryManager.AvailableDictionaries(ConfigManager.NativeLanguage);

            OnPropertyChanged(nameof(TargetLanguageDictionaries));
            OnPropertyChanged(nameof(NativeLanguageDictionaries));
        }

        public ICommand AddNewNativeDictionaryCommand { get; private set; }
        public ICommand AddNewTargetDictionaryCommand { get; private set; }
    }
}

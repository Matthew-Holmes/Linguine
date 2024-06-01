using Infrastructure;
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
                if (value == -1 || value == _targetLanguageIndex)
                {
                    // wpf likes trying to set this to -1 when the options are updating
                    return;
                }

                UpdateTargetLanguageInConfig(_languageCodes[value]);
                _targetLanguageIndex = value;

                OnPropertyChanged(nameof(TargetLanguage));

                _mainModel.Reload();
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

        #region dictionary loading
        public List<String> TargetLanguageDictionaries { get; private set; }

        public ICommand AddNewTargetDictionaryCommand { get; private set; }

        private void SetupDictionarySelection()
        {
            RefreshAvailableDictionaries();

            _mainModel.Reloaded += (s,e) => RefreshAvailableDictionaries();

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
                name = _uiComponents.CanGetText.GetResponse("Choose a name for the Dictioary");
                if (name is not null && name != "") { break; }
                if (_uiComponents.CanVerify.AskYesNo("Abort?")) { return; }
            }

            ExternalDictionaryManager? manager = _mainModel.ExternalDictionaryManager;

            if (manager is null)
            {
                _uiComponents.CanMessage.Show("Please wait for dictionary management to load");
                return;
            }
            else
            {
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
            TargetLanguageDictionaries = _mainModel.ExternalDictionaryManager?.AvailableDictionaries() ?? new List<String>();

            OnPropertyChanged(nameof(TargetLanguageDictionaries));
        }

        #endregion

        #region variants loading
        public List<String> TargetLanguageVariantSources { get; private set; }
        public ICommand AddNewTargetVariantsSourceCommand { get; private set; }

        private void SetupVariantsSelection()
        {
            RefreshAvailableVariantsSources();
            _mainModel.Reloaded += (s, e) => RefreshAvailableVariantsSources();

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
                name = _uiComponents.CanGetText.GetResponse("Choose a name for the Dictioary");
                if (name is not null && name != "") { break; }
                if (_uiComponents.CanVerify.AskYesNo("Abort?")) { return; }
            }

            VariantsManager? manager = _mainModel.VariantsManager;

            if (manager is null)
            {
                _uiComponents.CanMessage.Show("please wait for variabts manager to load");
            }
            else
            {
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
            TargetLanguageVariantSources = _mainModel.VariantsManager?.AvailableVariantsSources() ?? new List<String>();
            OnPropertyChanged(nameof(TargetLanguageVariantSources));
        }
        #endregion

        public ConfigManagerViewModel(UIComponents uiComponents, MainModel parent) : base(uiComponents, parent)
        {
            Title = "Config Manager";
            SetupLanguageSelection();
            SetupDictionarySelection();
            SetupVariantsSelection();
        }

       
    }
}

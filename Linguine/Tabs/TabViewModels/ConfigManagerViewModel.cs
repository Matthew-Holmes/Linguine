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

                // ********************* model replacement *******************
                ConfigManager.TargetLanguage = _languageCodes[value];
                _targetLanguageIndex = value;
                _parent.Model = new MainModel(); // build a new one
                // ******* this should be the only time it is changed ********

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

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
using System.Windows.Input;
using UserInputInterfaces;

namespace Linguine.Tabs
{
    public class ConfigManagerViewModel : TabViewModelBase
    {
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

        public  ConfigManagerViewModel(UIComponents uiComponents, MainModel parent) : base(uiComponents, parent)
        {
            Title = "Config Manager";

            _languageOptions = LanguageCodeDetails.LanguageNames(ConfigManager.NativeLanguage);

            _nativeLanguageIndex = _languageCodes.IndexOf(ConfigManager.NativeLanguage);
            _targetLanguageIndex = _languageCodes.IndexOf(ConfigManager.TargetLanguage);
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
    }
}

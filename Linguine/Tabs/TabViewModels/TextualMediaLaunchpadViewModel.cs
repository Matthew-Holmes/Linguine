using ExternalMedia;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows.Input;
using UserInputInterfaces;
using DataClasses;
using Config;

namespace Linguine.Tabs
{

    internal partial class TextualMediaLaunchpadViewModel : TabViewModelBase
    {
        private TextualMediaImporter _loader;

        private bool                        _showNewSessionButton = false;
        private string                      _selectedTextName;
        private List<string>                _availableSessions;
        private List<Tuple<bool, decimal>>  _sessionInfo;

        private ObservableCollection<ProcessingJobViewModel> _processingJobs = new();

        public ICommand ImportNewCommand { get; private set; }
        public ICommand NewSessionCommand { get; private set; }
        
        public ICommand DeleteSelectedTextualMediaCommand { get; private set; }

        public List<String> AvailableTexts => _model.AvailableTextualMediaNames;

        public String SelectedTextName
        {
            get => _selectedTextName;
            set
            {
                _selectedTextName = value;

                if (value == "")
                {
                    SessionInfo = new List<Tuple<bool, decimal>>();
                    AvailableSessions = new List<string>();
                    ShowSessions = false;
                } else
                {
                    LoadSessionsFor(value);
                    ShowSessions = true;
                }

                OnPropertyChanged(nameof(SelectedTextName));
                BulkProcessingViewLogicFor(value);
            }
        }

        private void BulkProcessingViewLogicFor(String textName)
        {
            if (textName == "")
            {
                _processingJobs = new ObservableCollection<ProcessingJobViewModel>();
                OnPropertyChanged(nameof(ProcessingJobs));
                return;
            } 

            if (ProcessingJobs.Any(vm => vm.TextName == textName))
            {
                return;
            }
            else
            {
                LanguageCode target = ConfigManager.Config.Languages.TargetLanguage;

                decimal timePerStep = ConfigManager.Config.Gimmicks.TimeToProcessSeconds[target];
                int charPerStep = ConfigManager.Config.Gimmicks.CharsProcessedPerStep[target];

                using var context = _model.ReadonlyLinguineFactory.CreateDbContext();
                MainModel.ProcessingJobInfo info = _model.GetProcessingInfo(
                    textName, false, timePerStep, charPerStep, context);
                var newVm = new ProcessingJobViewModel(_model, info);

                _processingJobs.Insert(0, newVm);
                OnPropertyChanged(nameof(ProcessingJobs));
            }
            
        }


        public bool ShowSessions
        {
            get => _showNewSessionButton;
            set
            {
                _showNewSessionButton = value;
                OnPropertyChanged(nameof(ShowSessions));

            }
        }

        private List<Tuple<bool, decimal>> SessionInfo
        { 
            get => _sessionInfo;
            set
            {
                AvailableSessions = BuildSessionStrings(SelectedTextName, value);
                _sessionInfo = value;
            }
        }

        public List<String> AvailableSessions
        {
            get => _availableSessions;
            set
            {
                _availableSessions = value;
                OnPropertyChanged(nameof(AvailableSessions));
            }
        }

        public ObservableCollection<ProcessingJobViewModel> ProcessingJobs => _processingJobs;

        public String SelectedSession
        {
            set
            {
                int index = AvailableSessions.IndexOf(value);

                if (index == -1)
                {
                    _uiComponents.CanMessage.Show("something went wrong!");
                    return;
                }

                decimal progress = SessionInfo[index].Item2;

                // TODO - having sessions receive a name would mean we didn't have to search them by progress

                bool success = _model.ActivateExistingSessionFor(SelectedTextName, progress);

                if (!success)
                {
                    _uiComponents.CanMessage.Show("activating session failed!");
                    return;
                }

                _parent.CloseThisAndSwitchToLatestSession(this);
            }
        }

        private void LoadSessionsFor(string name)
        {
            var info = _model.GetSessionInfoByName(name);

            if (info is null)
            {
                _uiComponents.CanMessage.Show("$failed to load sessions for {name}");
            }
            else
            {
                SessionInfo = info; 
            }
        }

        private List<String> BuildSessionStrings(String name, List<Tuple<bool, decimal>> info)
        {
            // need to decide whether to show active sessions, or just inactive ones

            List<String> ret = new List<String>();

            foreach(var sessionInfo in info)
            {
                StringBuilder builder = new StringBuilder(name);
                builder.Append(" | ");
                builder.Append(sessionInfo.Item2.ToString());
                builder.Append('%');

                ret.Add(builder.ToString());
            }

            return ret;
        }




        public TextualMediaLaunchpadViewModel(
            UIComponents uiComponents,
            MainModel model,
            MainViewModel parent) : base(uiComponents, model, parent)
        {
            _model.Loaded += (s, e) => OnPropertyChanged(nameof(AvailableTexts));

            _loader = new TextualMediaImporter(uiComponents.CanVerify, uiComponents.CanChooseFromList, uiComponents.CanGetText);

            Title = "Select Text";

            ImportNewCommand = new RelayCommand(() => ImportNew());

            NewSessionCommand = new RelayCommand(() => NewSession());

            DeleteSelectedTextualMediaCommand = new RelayCommand(() => DeleteSelectedTextualMedia());
     
            _processingJobs = new ObservableCollection<ProcessingJobViewModel>(
                _model.GetProcessingJobs()
                      .Select(info => new ProcessingJobViewModel(model, info))
                      .ToList());
        }

        private void DeleteSelectedTextualMedia()
        {
            if(_uiComponents.CanVerify.AskYesNo($"are you sure you want to delete {SelectedTextName}?"))
            {
                if (_uiComponents.CanVerify.AskYesNo($"are you really sure, this is IRREVERSIBLE"))
                {
                    if (_uiComponents.CanVerify.AskYesNo($"ok, select yes to delete {SelectedTextName}, no to abort"))
                    {
                        _model.DeleteTextualMedia(SelectedTextName);
                        _uiComponents.CanMessage.Show($"{SelectedTextName} was deleted");
                        OnPropertyChanged(nameof(AvailableTexts));
                        SelectedTextName = "";
                    }
                }
            }
        }

        private void NewSession()
        {
            if (_model.StartNewTextualMediaSession(SelectedTextName))
            {
                _parent.CloseThisAndSwitchToLatestSession(this);
                return;
            }

            _uiComponents.CanMessage.Show("something went wrong generating a session!");
        }

        private void ImportNew()
        {
            String filename = _uiComponents.CanBrowseFiles.Browse();

            try
            {
                TextualMedia tm = _loader.ImportFromFile(filename);

                var manager = _model.SM.Managers!.TextualMedia;

                if (manager is null)
                {
                    _uiComponents.CanMessage.Show("please wait for main model to load");
                    return;
                }

                // a bit of domain logic, but just checking the import file isn't a duplicate
                // asks the user to check if they think it is a duplicate in some edge cases

                if (IsADuplicate(tm, manager)) { return; }

                using var context = _model.LinguineFactory.CreateDbContext();

                manager.Add(tm, context);

                OnPropertyChanged(nameof(AvailableTexts));

                SelectedTextName = tm.Name;

                NewSession();

            }
            catch (Exception e)
            {
                _uiComponents.CanMessage.Show($"Media loading aborted: {e.Message}");
                return;
            }
        }

        private bool IsADuplicate(TextualMedia tm, TextualMediaManager manager)
        {
            using var context = _parent.Model.ReadonlyLinguineFactory.CreateDbContext();


            if (manager.AvailableTextualMediaNames(context).Contains(tm.Name))
            {
                _uiComponents.CanMessage.Show("Already have a text of this name!");
                return true;
            }

            if (manager.HaveMediaWithSameDescription(tm.Description, context))
            {
                if (!_uiComponents.CanVerify.AskYesNo("already have a text matching this description, proceed?"))
                {
                    return true;
                }
            }

            if (manager.HaveMediaWithSameContent(tm.Text, context))
            {
                _uiComponents.CanMessage.Show("already have media with exact same content, aborting!");
                return true;
            }

            return false;
        }

        private void BrowseAll()
        {
            // TODO - decide what to do if we don't want to just list all the files
            throw new NotImplementedException();
        }
    }
}

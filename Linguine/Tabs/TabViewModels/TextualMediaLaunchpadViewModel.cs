using ExternalMedia;
using Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using UserInputInterfaces;

namespace Linguine.Tabs
{
    
    internal class TextualMediaLaunchpadViewModel : TabViewModelBase
    {
        private MainViewModel _mainViewModel;
        private TextualMediaLoader _loader;

        private bool _showNewSessionButton = false;
        private string _selectedTextName;

        public ICommand ImportNewCommand { get; private set; }
        public ICommand NewSessionCommand { get; private set; }

        public List<String> AvailableTexts
        {
            get
            {
               return _mainModel.TextualMediaManager?.AvailableTextualMediaNames() ?? new List<String>();
            }
        }

        public String SelectedTextName
        {
            get => _selectedTextName;
            set
            {
                _selectedTextName = value;
                OnPropertyChanged(nameof(SelectedTextName));
                ShowSessions = true;
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

        public TextualMediaLaunchpadViewModel(
            UIComponents uiComponents,
            MainModel parent,
            MainViewModel mainViewModel) : base(uiComponents, parent)
        {
            _mainViewModel = mainViewModel; // so we can ask it to close this once done

            _loader = new TextualMediaLoader(uiComponents.CanVerify, uiComponents.CanChooseFromList, uiComponents.CanGetText);

            Title = "Select Text";

            ImportNewCommand = new RelayCommand(() => ImportNew());

            NewSessionCommand = new RelayCommand(() => NewSession());
        }

        private void NewSession()
        {
            if (_mainModel.StartNewTextualMediaSession(SelectedTextName))
            {
                _mainViewModel.CloseThisAndSwitchToLatestSession(this);
                return;
            }

            _uiComponents.CanMessage.Show("something went wrong generating a session!");
        }

        private void ImportNew()
        {
            String filename = _uiComponents.CanBrowseFiles.Browse();

            try
            {
                TextualMedia tm = _loader.LoadFromFile(filename);

                var manager = _mainModel.TextualMediaManager;

                if (manager is null)
                {
                    _uiComponents.CanMessage.Show("please wait for main model to load");
                    return;
                }

                // a bit of domain logic, but just checking the import file isn't a duplicate
                // asks the user to check if they think it is a duplicate in some edge cases

                if (IsADuplicate(tm, manager)) { return; }

                manager.Add(tm);

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
            if (manager.AvailableTextualMediaNames().Contains(tm.Name))
            {
                _uiComponents.CanMessage.Show("Already have a text of this name!");
                return false;
            }

            if (manager.HaveMediaWithSameDescription(tm.Description))
            {
                if (!_uiComponents.CanVerify.AskYesNo("already have a text matching this description, proceed?"))
                {
                    return false;
                }
            }

            if (manager.HaveMediaWithSameContent(tm.Text))
            {
                _uiComponents.CanMessage.Show("already have media with exact same content, aborting!");
                return false;
            }

            return true;
        }

        private void BrowseAll()
        {
            // TODO - decide what to do if we don't wan't to just list all the files
            throw new NotImplementedException();
        }
    }
    
}

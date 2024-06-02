using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserInputInterfaces;
using ExternalMedia;
using Infrastructure;
using Linguine.Tabs;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.Collections.Specialized;

namespace Linguine
{
    internal class MainViewModel : ViewModelBase
    {
        private UIComponents _UIcomponents;
        private MainModel _model;

        public ObservableCollection<TabViewModelBase> Tabs { get; private set; }
        public TabViewModelBase SelectedTab { get; set; }
        

        public MainViewModel(UIComponents uiComponents, MainModel model)
        {
            _UIcomponents = uiComponents;
            _model = model;

            model.LoadingFailed += (s,e) => uiComponents.CanMessage.Show("database loading failed");
            model.SessionsChanged += (s, e) => TurnSessionsToTabs();

            SetupTabs();
        }

        private void TurnSessionsToTabs()
        {
            List<TextualMediaViewerViewModel> existingTabs = Tabs
                .Where(t => t.GetType() == typeof(TextualMediaViewerViewModel))
                .Cast<TextualMediaViewerViewModel>()
                .ToList(); 

            var sessions = _model.ActiveSessions;

            // close non-existent tabs
            foreach (TextualMediaViewerViewModel tab in existingTabs)
            {
                if (!sessions.Contains(tab.Session))
                {
                    tab.CloseCommmand.Execute(this); // deactivates in the session database
                }
            }

            var existingSessions = existingTabs.Select(t => t.Session);

            foreach(TextualMediaSession session in sessions)
            {
                if (!existingSessions.Contains(session)) ;
                Add(new TextualMediaViewerViewModel(session, _UIcomponents, _model));   
            }

        }

        private void SetupTabs()
        {
            Tabs = new ObservableCollection<TabViewModelBase>();
            Tabs.CollectionChanged += Tabs_CollectionChanged;

            OpenHomeTabCommand               = new RelayCommand(() => Add(              new HomeViewModel(_UIcomponents, _model)));
            OpenTextualMediaViewerTabCommand = new RelayCommand(() => Add(new TextualMediaLaunchpadViewModel(_UIcomponents, _model, this)));
            OpenConfigManagerTabCommand      = new RelayCommand(() => AddUniquely<ConfigManagerViewModel>(_UIcomponents, _model));
        }

        private void SelectTab(TabViewModelBase tab)
        {
            SelectedTab = tab;
            OnPropertyChanged(nameof(SelectedTab));
        }

        #region open tab commands
        public ICommand OpenHomeTabCommand { get; private set; }
        public ICommand OpenConfigManagerTabCommand { get; private set; }
        public ICommand OpenTextualMediaViewerTabCommand { get; private set; }
        #endregion

        #region tab opening
        private void Add(TabViewModelBase viewModel)
        {
            Tabs.Add(viewModel);
            SelectTab(viewModel);
        }

        private void AddUniquely<T>(UIComponents ui, MainModel model) where T : TabViewModelBase
        {
            var existingTab = Tabs.OfType<T>().FirstOrDefault();
            if (existingTab == null)
            {
                var newTab = (T)Activator.CreateInstance(typeof(T), ui, model);
                Tabs.Add(newTab);
                SelectTab(newTab);
            }
            else
            {
                SelectTab(existingTab);
            }
        }
        #endregion

        #region tab closing
        private void OnTabClosed(object sender, EventArgs e)
        {
            if (sender is TabViewModelBase tab)
            {
                Tabs.Remove(tab);
            }
        }

        private void Tabs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.OldItems != null)
            {
                foreach (TabViewModelBase oldTab in e.OldItems)
                {
                    oldTab.TabClosed -= OnTabClosed;
                }
            }

            if (e.NewItems != null)
            {
                foreach (TabViewModelBase newTab in e.NewItems)
                {
                    newTab.TabClosed += OnTabClosed;
                }
            }
        }

        internal void CloseThisAndSwitchToLatestSession(TextualMediaLaunchpadViewModel textualMediaLaunchpadViewModel)
        {
            Tabs.Remove(textualMediaLaunchpadViewModel);

            List<TextualMediaViewerViewModel> existingSessionTabs = Tabs
               .Where(t => t.GetType() == typeof(TextualMediaViewerViewModel))
               .Cast<TextualMediaViewerViewModel>()
               .ToList();

            var latest = existingSessionTabs.OrderByDescending(t => t.Session.LastActive)
                .FirstOrDefault();

            if (latest is not null)
            {
                SelectedTab = latest;
            }
            else
            {
                _UIcomponents.CanMessage.Show("Switching to latest session failed!");
            }
        }
        #endregion
    }
}

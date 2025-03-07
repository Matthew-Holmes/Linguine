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
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows.Documents;
using System.Windows.Threading;
using Agents;

namespace Linguine
{
    public class MainViewModel : ViewModelBase
    {
        private UIComponents _UIcomponents;
        private MainModel? _model;
        private readonly SynchronizationContext _syncContext;

        public ObservableCollection<TabViewModelBase> Tabs { get; private set; } 
            = new ObservableCollection<TabViewModelBase>();
        public TabViewModelBase? SelectedTab { get; set; }
        
        // this property is responsible for replacing the underlying model
        public MainModel? Model
        {
            get => _model;
            set
            {
                // config manager is special, it will update its model and the MainViewModel's model 
                // kill the remaining tabs, those based off sessions will be revivable

                bool reopenConfigTab = false;
                foreach(TabViewModelBase tabViewModel in Tabs)
                {
                    if (tabViewModel.GetType() == typeof(ConfigManagerViewModel))
                    {
                        reopenConfigTab = true;
                    }
                    RunOnUIThread(() => Tabs.Remove(tabViewModel));
                }

                OpenTextualMediaViewerTabCommand = null; OnPropertyChanged(nameof(OpenTextualMediaViewerTabCommand));
                OpenHomeTabCommand               = null; OnPropertyChanged(nameof(OpenHomeTabCommand));
                OpenConfigManagerTabCommand      = null; OnPropertyChanged(nameof(OpenConfigManagerTabCommand));

                _model?.Dispose();
                _model = value;

                if (_model is not null)
                {
                    _model.LoadingFailed   += (s, e) => RunOnUIThread(() => DatabaseLoadingFailed());
                    _model.Loaded          += (s, e) => RunOnUIThread(() => SetupTabs());
                    _model.Loaded          += (s, e) => RunOnUIThread(() => TurnSessionsToTabs());
                    _model.SessionsChanged += (s, e) => RunOnUIThread(() => TurnSessionsToTabs());

                    if (reopenConfigTab)
                    {
                        _model.Loaded += (s, e) => RunOnUIThread(
                            () => AddUniquely<ConfigManagerViewModel>(_UIcomponents, _model, this));
                    }
                }
                // since tabs are what let us run model methods, if a tab is open, then there must be
                // an underlying loaded model

                _model?.BeginLoading(); // now we have all the event handler ready
            }
        }

        private void DatabaseLoadingFailed()
        {
            _UIcomponents.CanMessage.Show("database loading failed");
        }

        public MainViewModel(UIComponents uiComponents, MainModel model)
        {
            _UIcomponents = uiComponents;
            _syncContext  = SynchronizationContext.Current;

            Tabs.CollectionChanged += Tabs_CollectionChanged;

            Model = model;

            ConfigManager.ConfigChanged += (() => Model = new MainModel()); // reload main model on config change
        }

        private void RunOnUIThread(Action action)
        {

            if (_syncContext != null)
            {
                _syncContext.Post(_ => action(), null);
            }
            else
            {
                // Fallback: If _syncContext is null, execute the action directly
                action();
            }
        }

        private void TurnSessionsToTabs()
        {
            List<TextualMediaViewerViewModel> existingTabs = Tabs
                .Where(t => t.GetType() == typeof(TextualMediaViewerViewModel))
                .Cast<TextualMediaViewerViewModel>()
                .ToList();

            if (_model is null) { throw new Exception("model is null"); }

            var sessions = _model.ActiveSessionsIDs;

            // close non-existent tabs
            foreach (TextualMediaViewerViewModel tab in existingTabs)
            {
                if (!sessions.Contains(tab.SessionID))
                {
                    tab.CloseCommand.Execute(this); // deactivates in the session database
                }
            }

            var existingSessions = existingTabs.Select(t => t.SessionID);

            foreach(int session in sessions)
            {
                if (!existingSessions.Contains(session))
                {
                    Add(new TextualMediaViewerViewModel(session, _UIcomponents, _model, this));
                }
            }

        }

        private void SetupTabs()
        {
            OnPropertyChanged(nameof(Tabs));

            if (Model is null) { throw new Exception("mode is null"); }

            OpenHomeTabCommand = 
                new RelayCommand(() => Add(new HomeViewModel(_UIcomponents, Model, this)));
            OpenTextualMediaViewerTabCommand =
                new RelayCommand(() => Add(new TextualMediaLaunchpadViewModel(_UIcomponents, Model, this)));
            OpenConfigManagerTabCommand  = 
                new RelayCommand(() => AddUniquely<ConfigManagerViewModel>(_UIcomponents,Model, this));

            OnPropertyChanged(nameof(OpenHomeTabCommand));
            OnPropertyChanged(nameof(OpenTextualMediaViewerTabCommand));
            OnPropertyChanged(nameof(OpenConfigManagerTabCommand));
        }

        private void SelectTab(TabViewModelBase tab)
        {
            SelectedTab = tab;
            OnPropertyChanged(nameof(SelectedTab));
        }

        #region open tab commands
        public ICommand? OpenHomeTabCommand { get; private set; }
        public ICommand? OpenConfigManagerTabCommand { get; private set; }
        public ICommand? OpenTextualMediaViewerTabCommand { get; private set; }
        #endregion

        #region tab opening
        private void Add(TabViewModelBase viewModel)
        {
            Tabs.Add(viewModel);
            SelectTab(viewModel);
        }

        private void AddUniquely<T>(UIComponents ui, MainModel model, MainViewModel parent) where T : TabViewModelBase
        {
            var existingTab = Tabs.OfType<T>().FirstOrDefault();
            if (existingTab == null)
            {
                var newTab = (T)Activator.CreateInstance(typeof(T), ui, model, parent);
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
        #endregion

        #region tab switching

        internal void CloseThisAndSwitchToLatestSession(TextualMediaLaunchpadViewModel textualMediaLaunchpadViewModel)
        {
            // bit of a rough edge, required since the sync context messes up consecutive calls from other classes
            // when some events are invoked, this means that they get posted after if the invocation was before
            _syncContext.Post(_ => CloseThisAndSwitchToLatestSessionInternal(textualMediaLaunchpadViewModel), null);
        }

        private void CloseThisAndSwitchToLatestSessionInternal(TextualMediaLaunchpadViewModel textualMediaLaunchpadViewModel)
        {
            Tabs.Remove(textualMediaLaunchpadViewModel);

            List<TextualMediaViewerViewModel> existingSessionTabs = Tabs
               .Where(t => t.GetType() == typeof(TextualMediaViewerViewModel))
               .Cast<TextualMediaViewerViewModel>()
               .ToList();

            var latest = existingSessionTabs.OrderByDescending(t => _model.WhenLastActive(t.SessionID))
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

        internal void HandleMissingApiKeys(MissingAPIKeyException e)
        {
            _UIcomponents.CanMessage.Show(e.Message);
            _UIcomponents.CanMessage.Show("please check these files exist: " + e.missingLocation);
        }
        #endregion
    }
}

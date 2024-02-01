using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserInputInterfaces;
using ExternalMedia;
using LearningStore;
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

            SetupTabs();
        }

        private void SetupTabs()
        {
            Tabs = new ObservableCollection<TabViewModelBase>();
            Tabs.CollectionChanged += Tabs_CollectionChanged;

            OpenHomeTabCommand               = new RelayCommand(() => Add(              new HomeViewModel(_UIcomponents, _model)));
            OpenTextualMediaViewerTabCommand = new RelayCommand(() => Add(new TextualMediaViewerViewModel(_UIcomponents, _model)));
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
        #endregion
    }
}

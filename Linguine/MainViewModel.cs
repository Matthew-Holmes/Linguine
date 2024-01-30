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

        public ICommand OpenHomeTabCommand { get; private set; }
        public ICommand OpenConfigManagerTabCommand { get; private set; }
        public ICommand OpenTextualMediaViewerTabCommand { get; private set; }

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

            OpenHomeTabCommand = new RelayCommand(() => Tabs.Add(new HomeViewModel(_UIcomponents, _model)));
            OpenConfigManagerTabCommand = new RelayCommand(() => Tabs.Add(new ConfigManagerViewModel(_UIcomponents, _model)));
            OpenTextualMediaViewerTabCommand = new RelayCommand(() => Tabs.Add(new TextualMediaViewerViewModel(_UIcomponents, _model)));
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

        private void OnTabClosed(object sender, EventArgs e)
        {
            if (sender is TabViewModelBase tab)
            {
                Tabs.Remove(tab);
            }
        }
    }
}

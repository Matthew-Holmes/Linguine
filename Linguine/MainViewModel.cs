using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserInputInterfaces;
using ExternalMedia;

namespace Linguine
{
    internal class MainViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        private UIComponents _UIcomponents;
        private MainModel _model;

        public RelayCommand LoadTextualMediaCommand { get; }

        public MainViewModel(UIComponents uiComponents, MainModel model)
        {
            _UIcomponents = uiComponents;
            _model = model;
        }
    }
}

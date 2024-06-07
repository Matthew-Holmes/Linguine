using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UserInputInterfaces;

namespace Linguine.Tabs
{
    public abstract class TabViewModelBase : ViewModelBase
    {
        // base class for anything that can exist as a distinct tab/window

        protected UIComponents _uiComponents;
        protected MainViewModel _parent;
        protected MainModel _model;

        public String Title { get; protected set; }

        public ICommand CloseCommand { get; private set; }

        public event EventHandler TabClosed;

        internal TabViewModelBase(UIComponents uiComponents, MainModel model, MainViewModel parent)
        {
            _uiComponents = uiComponents;
            _parent = parent;
            _model = model;

            CloseCommand = new RelayCommand(() => TabClosed?.Invoke(this, EventArgs.Empty));
        }
    }
}

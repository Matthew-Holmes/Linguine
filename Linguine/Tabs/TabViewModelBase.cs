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
        protected MainModel _mainModel;

        public String Title { get; protected set; }

        public ICommand CloseCommmand { get; private set; }

        public event EventHandler TabClosed;

        public TabViewModelBase(UIComponents uiComponents, MainModel parent)
        {
            _uiComponents = uiComponents;
            _mainModel = parent; // maybe this should be the viewmodel, will have to decide later

            CloseCommmand = new RelayCommand(() => TabClosed?.Invoke(this, EventArgs.Empty));
        }
    }
}

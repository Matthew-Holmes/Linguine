using UserInputInterfaces;

namespace Linguine.Tabs
{
    internal class HomeViewModel : TabViewModelBase
    {
        public HomeViewModel(
            UIComponents uiComponents,
            MainModel model,
            MainViewModel parent) : base(uiComponents, model, parent)
        {
            Title = "Home";
        }
    }
}

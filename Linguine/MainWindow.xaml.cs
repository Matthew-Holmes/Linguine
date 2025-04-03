using System.Windows;
using UserInputInterfaces;

namespace Linguine
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            UIComponents uiComponents = new UIComponents(
                new FileBrowserService(),
                new UserSelectionServiceWPF(),
                new UserInteractionServiceWPF(),
                new UserResponseService(),
                new MessageUserService()
                ); ; ;

            MainModel model = new();
     
            this.DataContext = new MainViewModel(uiComponents, model);
        }
    }
}

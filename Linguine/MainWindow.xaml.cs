using Infrastructure;
using Linguine.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
                new UserInteractionServiceWPF()
                );

            MainModel model;
            try
            {
                model = new MainModel();
            }
            catch (FileNotFoundException e)
            {
                if (MissingConfigHelper.TryFindConfig(uiComponents))
                { 
                    /* success */
                } else
                {
                    throw new Exception("Startup failed due to lack of config!");
                }

                model = new MainModel();
            }
            this.DataContext = new MainViewModel(uiComponents, model);
        }
    }
}

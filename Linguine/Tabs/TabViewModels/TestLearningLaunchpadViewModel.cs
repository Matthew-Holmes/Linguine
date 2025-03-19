using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserInputInterfaces;

namespace Linguine.Tabs 
{ 
    public class TestLearningLaunchpadViewModel : TabViewModelBase
    {
        public TestLearningLaunchpadViewModel(UIComponents uiComponents, MainModel model, MainViewModel parent) : base(uiComponents, model, parent)
        {
            Title = "Test";
        }
    }
}

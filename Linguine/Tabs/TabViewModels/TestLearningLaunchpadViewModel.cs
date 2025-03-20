using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using UserInputInterfaces;

namespace Linguine.Tabs 
{ 
    public class TestLearningLaunchpadViewModel : TabViewModelBase
    {
        public ICommand FreeStudyCommand     { get; private set; }
        public ICommand TargetedStudyCommand { get; private set; }
        public TestLearningLaunchpadViewModel(UIComponents uiComponents, MainModel model, MainViewModel parent) : base(uiComponents, model, parent)
        {
            Title = "Test";

            FreeStudyCommand     = new RelayCommand(() => BeginFreeStudy());
            TargetedStudyCommand = new RelayCommand(() => BeginTargetedStudy());
        }

        private void BeginTargetedStudy()
        {
            _parent.CloseThisAndBeginTargetedStudy(this);
        }

        private void BeginFreeStudy()
        {
            _parent.CloseThisAndBeginFreeStudy(this);
        }
    }
}

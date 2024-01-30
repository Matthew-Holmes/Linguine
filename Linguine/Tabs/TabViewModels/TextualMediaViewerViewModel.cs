using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserInputInterfaces;

namespace Linguine.Tabs
{
    internal class TextualMediaViewerViewModel : TabViewModelBase
    {
        public TextualMediaViewerViewModel(UIComponents uiComponents, MainModel parent) : base(uiComponents, parent)
        {
            Title = "Text Viewer";
        }
    }
}

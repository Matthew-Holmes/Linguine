using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserInputInterfaces;
using DataClasses;

namespace Linguine.Tabs
{
    class DefinitionRepairViewModel : TabViewModelBase
    {
        public DefinitionRepairViewModel(DictionaryDefinition faulty, UIComponents uiComponents, MainModel model, MainViewModel parent) : base(uiComponents, model, parent)
        {
            Title = "Repair Definition";
        }
    }
}

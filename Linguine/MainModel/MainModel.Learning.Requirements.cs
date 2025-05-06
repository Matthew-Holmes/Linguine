using Learning;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Linguine
{
    partial class MainModel
    {
        // TODO - get the service manager to handle if we need to import things
        // perhaps use a state machine??
        public bool NeedToImportADictionary { get; set; } = true;

        internal DLSRequirements GetDLSRequirements()
        {
            return DefLearningService.RequirementsMet();
        }
    }
}

using Infrastructure;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserInputInterfaces;

namespace Linguine.Helpers
{
    internal static class MissingConfigHelper
    {
        internal static bool TryFindConfig(UIComponents uiComponents)
        {
            if (uiComponents.CanVerify.AskYesNo("No config located, browse for file?"))
            {
                string configPath = uiComponents.CanBrowseFiles.Browse();

                if (!string.IsNullOrEmpty(configPath))
                {
                    if (ConfigFileHandler.SetCustomConfig(configPath))
                    {
                        return true;
                    }
                }
                if (uiComponents.CanVerify.AskYesNo("Invalid config, try again?"))
                {
                    return TryFindConfig(uiComponents);
                }
            }

            if (uiComponents.CanVerify.AskYesNo("Generate default config?"))
            {
                ConfigFileHandler.SetConfigToDefault();
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}

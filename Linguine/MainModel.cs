using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;

namespace Linguine
{
    internal class MainModel
    {
        public MainModel()
        {
            if (ConfigManager.LoadConfig() is null)
            {
                // this is the only exception that should be possibly to be thrown during main model construction
                throw new FileNotFoundException("Couldn't find config");
            }
        }
    }
}

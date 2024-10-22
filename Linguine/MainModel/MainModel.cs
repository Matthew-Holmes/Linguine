using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Emit;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

//using Agents;
//using Agents.DummyAgents;
using Infrastructure;
using Linguine.Tabs;
using UserInputInterfaces;
//using LearningExtraction;

namespace Linguine
{
    public partial class MainModel : IDisposable
    {
        public event EventHandler? Loaded;
        public event EventHandler? LoadingFailed;

        private String _linguineConnectionString;


        public MainModel()
        {
            if (!ConfigFileHandler.LoadConfig())
            {
                // this is the only exception that should be possibly to be thrown during main model construction
                throw new FileNotFoundException("Couldn't find config");
            }
            _linguineConnectionString = ConfigManager.ConnectionString;
        }

        public void BeginLoading()
        {
            Task.Run(() => Load()); // load in background
        }

        private void Load()
        {
            try
            {
                if (ConfigManager.DatabaseDirectory != null)
                {
                    Directory.CreateDirectory(ConfigManager.DatabaseDirectory);
                }

                using LinguineContext lg = new LinguineContext(_linguineConnectionString);
                lg.Database.EnsureCreated();

                LoadManagers(); 

                Loaded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                LoadingFailed?.Invoke(this, EventArgs.Empty);
            }
        }

        public void Dispose()
        {
            // clean up our events
            foreach (Delegate d in LoadingFailed.GetInvocationList())
            {
                LoadingFailed -= (EventHandler)d;
            }
            foreach (Delegate d in Loaded.GetInvocationList())
            {
                Loaded -= (EventHandler)d;
            }
            foreach (Delegate d in SessionsChanged.GetInvocationList())
            {
                SessionsChanged -= (EventHandler)d;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        
        private LinguineDataHandler? _linguine;
        private LinguineDataHandler Linguine
        {
            get
            {
                if (_linguine is null)
                {
                    throw new Exception("attempting to access database before model loaded");
                }
                return _linguine;
            }
            set
            {
                if (value is null)
                {
                    throw new Exception("don't set the database to null");
                }
                _linguine = value;
            }
        }

        public MainModel()
        {
            if (!ConfigFileHandler.LoadConfig())
            {
                // this is the only exception that should be possibly to be thrown during main model construction
                throw new FileNotFoundException("Couldn't find config");
            }
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

                Linguine = new LinguineDataHandler(ConfigManager.ConnectionString);
                Linguine.Database.EnsureCreated();

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
            Linguine.SaveChanges();
            Linguine.Dispose();
        }







        /*
        public TextDecomposer? TextDecomposer { get; private set; } = null;
        public UnitRooter? UnitRooter { get; private set; } = null;
        public DefinitionResolver? DefinitionResolver { get; private set; } = null;

        public bool LoadTextDecompositionService()
        {
            String apiKey = File.ReadLines(ConfigManager.OpenAI_APIKey).First();

            TextDecomposer = new TextDecomposer();

            TextDecomposer.StandardAgent = new TextDecompositionAgent(apiKey, highPowered: false);
            TextDecomposer.HighPerformanceAgent = new TextDecompositionAgent(apiKey, highPowered: true);
            TextDecomposer.FallbackAgent = new WhitespaceDecompositionAgent();

            UnitRooter = new UnitRooter();
            UnitRooter.Agent = new UnitRootingAgent(apiKey);

            return true;
        }

        public bool LoadDefinitionResolutionService(ExternalDictionary dictionary)
        {
            DefinitionResolver = new DefinitionResolver();
            DefinitionResolver.Dictionary = dictionary;

            String apiKey = File.ReadLines(ConfigManager.OpenAI_APIKey).First();

            DefinitionResolver.Agent = new DefinitionResolutionAgent(apiKey);   

            return true;
        }
        */
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
//using Agents;
//using Agents.DummyAgents;
using Infrastructure;
using Linguine.Tabs;
using UserInputInterfaces;
//using LearningExtraction;

namespace Linguine
{
    public partial class MainModel
    {

        public bool StartupComplete { get; private set; }

        public event EventHandler Reloaded;
        public event EventHandler LoadingFailed;

        private LinguineDataHandler Linguine { get; set; }


        public MainModel()
        {
            if (!ConfigFileHandler.LoadConfig())
            {
                // this is the only exception that should be possibly to be thrown during main model construction
                throw new FileNotFoundException("Couldn't find config");
            }

            Task.Run(() => Reload()); // load in background
            
        }

        public void Reload()
        {
            try
            {
                StartupComplete = false;

                if (ConfigManager.DatabaseDirectory != null)
                {
                    Directory.CreateDirectory(ConfigManager.DatabaseDirectory);
                }

                if (Linguine is not null)
                {
                    Linguine.SaveChanges();
                    Linguine.Dispose();
                }

                Linguine = new LinguineDataHandler(ConfigManager.ConnectionString);
                Linguine.Database.EnsureCreated();

                LoadManagers();

                StartupComplete = true;
                Reloaded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                LoadingFailed.Invoke(this, EventArgs.Empty);
            }
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

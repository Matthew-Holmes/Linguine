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
//using LearningExtraction;
using LearningStore;

namespace Linguine
{
    public class MainModel
    {
        public bool StartupComplete { get; private set; }
        public event EventHandler Reloaded;
        public event EventHandler LoadingFailed;

        private LinguineDbContext Linguine { get; set; }


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

                Linguine = new LinguineDbContext(ConfigManager.ConnectionString);
                Linguine.Database.EnsureCreated();
                StartupComplete = true;
                Reloaded?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception e)
            {
                LoadingFailed.Invoke(this, EventArgs.Empty);
            }
        }

        public ExternalDictionaryManager? ExternalDictionaryManager
        {
            get 
            {            
                if (StartupComplete)
                {
                    return new ExternalDictionaryManager(Linguine);
                }
                else
                {
                    return null;
                }
            }
        }
        public VariantsManager? VariantsManager
        {
            get
            {
                if (StartupComplete)
                {
                    return new VariantsManager(Linguine);
                }
                else
                {
                    return null;
                }
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

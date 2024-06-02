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
using UserInputInterfaces;
//using LearningExtraction;

namespace Linguine
{
    public class MainModel
    {
        #region managers
        private TextualMediaManager? _textualMediaManager;
        private ExternalDictionaryManager? _externalDictionaryManager;
        private VariantsManager? _variantsManager;
        private TextualMediaSessionManager? _textualMediaSessionManager;

        private void LoadManagers()
        {
            _externalDictionaryManager = new ExternalDictionaryManager(Linguine);
            _textualMediaManager = new TextualMediaManager(Linguine);
            _textualMediaSessionManager = new TextualMediaSessionManager(Linguine);
            _variantsManager = new VariantsManager(Linguine);
        }

        public TextualMediaManager? TextualMediaManager
        {
            get
            {
                if (StartupComplete)
                {
                    return _textualMediaManager;
                }
                else
                {
                    return null;
                }
            }
        }

        public ExternalDictionaryManager? ExternalDictionaryManager
        {
            get
            {
                if (StartupComplete)
                {
                    return _externalDictionaryManager;
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
                    return _variantsManager;
                }
                else
                {
                    return null;
                }
            }
        }

        private TextualMediaSessionManager? TextualMediaSessionManager
        {
            get
            {
                if (StartupComplete)
                {
                    return _textualMediaSessionManager;
                }
                else
                {
                    return null;
                }
            }
        }
        #endregion

        public bool StartupComplete { get; private set; }


        public event EventHandler Reloaded;
        public event EventHandler LoadingFailed;


        public event EventHandler SessionsChanged;

        private LinguineDataHandler Linguine { get; set; }

        public List<TextualMediaSession> ActiveSessions 
        {
            get => TextualMediaSessionManager?.ActiveSessions() ?? new List<TextualMediaSession>();
        }


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

        public List<string>? AvailableTextualMediaNames
        {
            get => _textualMediaManager?.AvailableTextualMediaNames() ?? null;
        }


        internal bool StartNewTextualMediaSession(string selectedText)
        {
            var tm = TextualMediaManager?.GetByName(selectedText) ?? null;

            if (tm is null)
            {
                return false;
            }

            if(TextualMediaSessionManager?.NewSession(tm) ?? false)
            {
                SessionsChanged?.Invoke(this, EventArgs.Empty);
                return true;
            }

            return false;
        }

        internal void CloseSession(TextualMediaSession session)
        {
            TextualMediaSessionManager?.CloseSession(session); // not the end of the world if we don't have it, 
        }

        internal List<Tuple<bool,decimal>>? GetSessionInfoByName(string name)
        {
            TextualMedia? tm = TextualMediaManager?.GetByName(name) ?? null;

            if (tm is null) { return null; }

            return TextualMediaSessionManager?.SessionInfo(tm) ?? null;
        }

        internal bool ActivateExistingSessionFor(string selectedTextName, decimal progress)
        {
            TextualMedia? tm = TextualMediaManager?.GetByName(selectedTextName) ?? null;

            if (tm is null) { return false; }

            bool b = TextualMediaSessionManager?.ActivateExistingSession(tm, progress) ?? false;

            if (b)
            {
                SessionsChanged?.Invoke(this, EventArgs.Empty);
            }

            return b;

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

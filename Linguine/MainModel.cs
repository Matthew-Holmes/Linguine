using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using Agents;
using Agents.DummyAgents;
using Infrastructure;
using LearningExtraction;

namespace Linguine
{
    public class MainModel
    {
        public MainModel()
        {
            if (!ConfigFileHandler.LoadConfig())
            {
                // this is the only exception that should be possibly to be thrown during main model construction
                throw new FileNotFoundException("Couldn't find config");
            }
        }

        public TextDecomposer? TextDecomposer { get; private set; } = null;
        public CaseNormaliser? CaseNormaliser { get; private set; } = null;

        public UnitRooter?     UnitRooter { get; private set; } = null;

        public bool LoadTextDecompositionService()
        {
            String apiKey = File.ReadLines(ConfigManager.OpenAI_APIKey).First();

            TextDecomposer = new TextDecomposer();

            TextDecomposer.StandardAgent = new TextDecompositionAgent(apiKey, highPowered: false);
            TextDecomposer.HighPerformanceAgent = new TextDecompositionAgent(apiKey, highPowered: true);
            TextDecomposer.FallbackAgent = new WhitespaceDecompositionAgent();

            CaseNormaliser = new CaseNormaliser();
            CaseNormaliser.Agent = new CaseNormalisationAgent(apiKey);

            UnitRooter = new UnitRooter();
            UnitRooter.Agent = new UnitRootingAgent(apiKey);

            return true;
        }



    }
}

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

        public bool LoadTextDecompositionService()
        {
            String apiKey = File.ReadLines(ConfigManager.OpenAI_APIKey).First();

            TextDecompositionAgent agent = new TextDecompositionAgent(apiKey);

            // upper bound max volume to process since #tokens <= #chars
            TextDecomposer = new TextDecomposer(agent.MaxTokens - agent.PreambleCharCount, agent); // TODO - this should be a factory?
            return true;
        }


    }
}

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
using LearningStore;

namespace Linguine
{
    public class MainModel
    {
        public Variants? TargetLanguageVariants { get; private set; }
        public bool TargetLanguageVariantsLoaded { get; private set; } = false; // for lazy loading

        public MainModel()
        {
            if (!ConfigFileHandler.LoadConfig())
            {
                // this is the only exception that should be possibly to be thrown during main model construction
                throw new FileNotFoundException("Couldn't find config");
            }
            ConfigFileHandler.ConfigUpdated += OnConfigUpdated;
        }

        private async void OnConfigUpdated()
        {
            await LoadTargetLanguageVariants();
            TargetLanguageVariantsLoaded = true;
        }

        private async Task LoadTargetLanguageVariants()
        {
            var possibilities = ConfigManager.SavedVariantsNamesAndConnnectionStrings[ConfigManager.TargetLanguage];
            if (possibilities.Count > 1)
            {
                throw new NotImplementedException("Can't currently use multiple sources of variant/root lookup");
            }

            String name = possibilities.First().Item1;
            String conn = possibilities.First().Item2;

            await Task.Run(() => TargetLanguageVariants = new Variants(ConfigManager.TargetLanguage, name, conn));
        }

        // ********************************************************************
        //                      Text Decomposition
        // ********************************************************************

        public TextDecomposer? TextDecomposer { get; private set; } = null;

        public bool LoadTextDecompositionService()
        {
            String apiKey = File.ReadLines(ConfigManager.OpenAI_APIKey).First();

            TextDecompositionAgent agent = new TextDecompositionAgent(apiKey);

            // upper bound max volume to process since #tokens <= #chars
            TextDecomposer = new TextDecomposer(agent.MaxTokens - agent.PreambleCharCount, agent); // TODO - this should be a factory?
            return true;
        }


        // ********************************************************************
        //                      Root Resolution
        // ********************************************************************

        public RootResolver? RootResolver { get; private set; } = null;

        public async Task<bool> LoadRootResolutionService()
        {
            String apiKey = File.ReadLines(ConfigManager.OpenAI_APIKey).First();

            RootChooserAgent chooser = new RootChooserAgent(apiKey);
            RootValidatorAgent validator = new RootValidatorAgent(apiKey);
            RootGeneratorAgent generator = new RootGeneratorAgent(apiKey);

            if (!TargetLanguageVariantsLoaded)
            {
                await LoadTargetLanguageVariants();
            }

            RootResolver = new RootResolver(50, 20, TargetLanguageVariants);

            RootResolver.ResolutionOptionsChooser = chooser;
            RootResolver.ResolutionValidator      = validator;
            RootResolver.ResolutionGenerator      = generator;

            return true;
        }

        // ********************************************************************
        //              Text Decomposition and Root Resolution
        // ********************************************************************

        public TextDecomposerAndRooter TextDecomposerAndRooter { get; private set; } = null;

        public bool LoadTextDecompositionAndRootingService()
        {
            String apiKey = File.ReadLines(ConfigManager.OpenAI_APIKey).First();

            TextDecompositionAndRootingAgent agent = new TextDecompositionAndRootingAgent(apiKey);

            // upper bound max volume to process since #tokens <= #chars
            TextDecomposerAndRooter = new TextDecomposerAndRooter(agent.MaxTokens - agent.PreambleCharCount, agent); // TODO - this should be a factory?
            return true;
        }

    }
}

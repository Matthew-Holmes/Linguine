using Config;
using DataClasses;
using Infrastructure;
using Learning.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning
{
    public static class DefinitionLearningServiceFactory
    {
        // requirements are
        // VocabModel needs FrequencyData
        // Strategist needs VocabModel (for zipf scores and PKnown)
        // Tactician needs Strategist
        // DFLS needs Tactician and FreqencyData (for occurence multiplier) 

        public static IDefinitionLearningService BuildDLS(
            FrequencyData freqData, List<TestRecord> allRecords, CancellationTokenSource cts)
        {
            List<List<TestRecord>>     sessions = LearningTacticsHelper.GetSessions(allRecords);
            List<DictionaryDefinition> distinct = DistinctDefinitionTested(allRecords);


            VocabularyModel vocabModel = new VocabularyModel(freqData, allRecords);
            Strategist      strategist = new Strategist(vocabModel, sessions, distinct, cts);
            Tactician       tactician  = new Tactician(strategist, sessions, cts);

            MakeDebugPlots(strategist, tactician);

            return new DefinitionLearningService
            {
                AllRecords  = allRecords,
                Frequencies = freqData,
                VocabModel  = vocabModel,
                Strategist  = strategist,
                Tactician   = tactician
            };
        }

        private static List<DictionaryDefinition> DistinctDefinitionTested(List<TestRecord> allRecords)
        {
            return allRecords.GroupBy(tr => tr.DictionaryDefinitionKey)
                             .Select(grouping => grouping.First().Definition)
                             .ToList();
        }

        private static void MakeDebugPlots(Strategist strategist, Tactician tactician)
        {
            new Thread(() =>
            {

                HashSet<int> defKeysPlotted = new HashSet<int>();

                var toIt = new List<DefinitionFeatures>(strategist.ModelData.distinctDefinitionFeatures);
                toIt.Reverse(); // more interesting examples at the end of this

                foreach (DefinitionFeatures feature in toIt)
                {
                    int key = feature.def.DatabasePrimaryKey;

                    if (!defKeysPlotted.Add(key))
                        continue;

                    string name = key + feature.def.Word;
                    string language = ConfigManager.Config.Languages.TargetLanguage.ToString();

                    Tuple<LearningTactic, DateTime> lastSeen = strategist.ModelData.distinctDefinitionsLastTacticUsed[key];

                    ProbabilityPlotter.PlotProbabilityCurves(
                        strategist.Model, feature, strategist.TacticsUsed, lastSeen.Item1, lastSeen.Item2,
                        $"plots/{language}/{name}.png");

                    tactician.PlotMDP(key,
                                    $"plots/{language}/{feature.def.DatabasePrimaryKey}_mdp.png",
                                    $"plots/{language}/{feature.def.DatabasePrimaryKey}_exploded_mdp.png");
                }
            })
            {
                IsBackground = true
            }.Start();
        }
    }
}

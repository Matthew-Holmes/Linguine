using DataClasses;
using Learning.LearningTacticsRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Security;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.Optimization;
using Learning.Strategy;
using System.Reflection.Metadata;
using System.ComponentModel;
using Config;
using System.Runtime.CompilerServices;
using HarfBuzzSharp;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Serilog;

namespace Learning
{
    internal partial class Strategist
    {
        // Agent that aims to maximise the probability of correct response on first try
        // using discounted rewards looking into the future

        // sees a history of past learning tactics and models this probability into the future
        // do this via a latent embedding of the word, with the latent embedding produced by the history

        // then will be presented with variety of options by the Tactician
        // and choose the one with highest increase in future reward

        // most of the model should be based off of the last session
        // since this is what we can control when planning the next session


        internal LearningTacticsHelper TacticsHelper = new LearningTacticsHelper();
        internal List<Type> TacticsUsed { get; private set; }
        internal IReadOnlyDictionary<int, DefinitionFeatures> DefFeatures { get; private set; }
        internal IReadOnlyDictionary<int, Tuple<LearningTactic, DateTime>> LastTacticUsedForDefinition { get; private set; }
        internal IReadOnlyDictionary<Type, double> DefaultRewards { get; set; }
        internal double BaseLineReward { get; set; }
        internal double BaseLineInitialPKnown { get; set; }

        private LogisticRegression Model { get; set; }
        internal VocabularyModel VocabModel { get; init; }


        internal Strategist(VocabularyModel vocab)
        {
            VocabModel = vocab;
        }

        internal Tactician BuildModel(List<List<TestRecord>> sessions, List<DictionaryDefinition> defs)
        {
            ModelData modelData = GetDataForModel(sessions, defs);

            TacticsUsed                 = modelData.tacticsUsed;
            DefFeatures                 = modelData.defFeaturesLookup.AsReadOnly();
            LastTacticUsedForDefinition = modelData.distinctDefinitionsLastTacticUsed.AsReadOnly();
            DefaultRewards              = modelData.followingSessionAverages.AsReadOnly(); ;
            BaseLineReward              = modelData.tacticAverageReward;

            Model = new LogisticRegression(
                            modelData.trainingData, modelData.tacticsUsed);

            Tactician tactics = new Tactician(this, sessions);

            // just for debug
            new Thread(() =>
            {
                HashSet<int> defKeysPlotted = new HashSet<int>();

                foreach (DefinitionFeatures feature in modelData.distinctDefinitionFeatures)
                {
                    int key = feature.def.DatabasePrimaryKey;

                    if (!defKeysPlotted.Add(key))
                        continue;

                    string name = key + feature.def.Word;
                    string language = ConfigManager.Config.Languages.TargetLanguage.ToString();

                    ProbabilityPlotter.PlotProbabilityCurves(Model, feature, TacticsUsed, $"plots/{language}/{name}.png");

                    tactics.PlotMDP(key, 
                                    $"plots/{language}/{feature.def.DatabasePrimaryKey}_mdp.png", 
                                    $"plots/{language}/{feature.def.DatabasePrimaryKey}_exploded_mdp.png");
                }
            })
            {
                IsBackground = true
            }.Start();

            return tactics;
        }

        internal double PredictProbability(FollowingSessionDatum input)
        {
            return Model.PredictProbability(input, TacticsUsed);
        }

        internal double GetExistingPKnown(int defKey, double lookAheadDays)
        {
            // predicts the probability we will know the definition in one day
            // todo - use discounted return?

            FollowingSessionDatum? input = GetCurrentRewardFeatures(defKey, lookAheadDays);

            if (input is null)
            {
                return VocabModel.PKnownWithError[defKey].Item1;
            } else
            {
                return PredictProbability(input);
            }
        }

        internal FollowingSessionDatum? GetCurrentRewardFeatures(int key, double lookAheadDays)
        {
            if (!LastTacticUsedForDefinition.ContainsKey(key))
            {
                return null;
            }

            if (LastTacticUsedForDefinition[key] is null)
            {
                return null;
            }

            LearningTactic lastTactic = LastTacticUsedForDefinition[key].Item1;
            DateTime when = LastTacticUsedForDefinition[key].Item2;

            Type lastTacticType = lastTactic.GetType();

            if (!TacticsUsed.Contains(lastTacticType))
            {
                Log.Warning("encountered last tactic type that was not logged as used");
                return null;
            }

            TimeSpan interval = DateTime.Now - when;
            double intervalDays = interval.TotalDays + lookAheadDays;

            DefinitionFeatures features = DefFeatures[key];

            return CreateDatum(features, lastTacticType, intervalDays);

        }
    }
}

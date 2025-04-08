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
    partial class Strategist
    {
        // Agent that aims to maximise the probability of correct response on first try
        // using discounted rewards looking into the future

        // sees a history of past learning tactics and models this probability into the future
        // do this via a latent embedding of the word, with the latent embedding produced by the history

        // then will be presented with variety of options by the Tactician
        // and choose the one with highest increase in future reward

        // most of the model should be based off of the last session
        // since this is what we can control when planning the next session


        public LearningTacticsHelper TacticsHelper = new LearningTacticsHelper();

        public LogisticRegression Model { get; private set; }
        public IReadOnlyDictionary<int, DefinitionFeatures> DefFeatures { get; private set; }
        public List<Type> TacticsUsed { get; private set; }

        public IReadOnlyDictionary<int, Tuple<LearningTactic, DateTime>> LastTacticUsedForDefinition { get; private set; }

        public void BuildModel(List<List<TestRecord>> sessions, List<DictionaryDefinition> defs)
        {
            ModelData modelData = GetDataForModel(sessions, defs);
            LogisticRegression model = new LogisticRegression(
                modelData.trainingData, modelData.tacticsUsed);

            TacticsUsed = modelData.tacticsUsed;
            DefFeatures = modelData.defFeaturesLookup.AsReadOnly();
            LastTacticUsedForDefinition = modelData.distinctDefinitionsLastTacticUsed.AsReadOnly();
            Model = model;

            // just for debug
            HashSet<int> defKeysPlotted = new HashSet<int>();
            foreach (DefinitionFeatures feature in modelData.distinctDefinitionFeatures)
            {
                int key = feature.def.DatabasePrimaryKey;

                if (defKeysPlotted.Contains(key))
                {
                    continue;
                }
                else
                {
                    defKeysPlotted.Add(key);
                }

                string name = feature.def.Word + feature.def.DatabasePrimaryKey;
                string language = ConfigManager.Config.Languages.TargetLanguage.ToString();

                ProbabilityPlotter.PlotProbabilityCurves(model, feature, TacticsUsed, $"plots/{language}/{name}.png");
            }

            Tactician tactics = new Tactician(this);

            tactics.BuildMarkovModel(sessions);

        }

        public FollowingSessionDatum? GetFeaturesForReward(int key, double lookAheadDays = 1.0)
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
                Log.Warning("encoured last tactic type that was not logged as used");
                return null;
            }

            TimeSpan interval = DateTime.Now - when;
            double intervalDays = interval.TotalDays + lookAheadDays;

            DefinitionFeatures features = DefFeatures[key];

            return CreateDatum(features, lastTacticType, intervalDays);

        }
    }
}

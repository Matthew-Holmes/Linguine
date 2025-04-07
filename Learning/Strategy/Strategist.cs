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


        private LearningTactics Tactics = new LearningTactics();


        public LogisticRegression Model { get; private set; }
        public IReadOnlyDictionary<int, DefinitionFeatures> DefFeatures { get; private set; }
        public List<Type> TacticsUsed { get; private set; }

        public IReadOnlyDictionary<int, Tuple<LearningTactic, DateTime>> LastTacticUsed { get; private set; }

        public void BuildModel(List<List<TestRecord>> sessions, List<DictionaryDefinition> defs)
        {
            (List<FollowingSessionDatum> data,
                List<DefinitionFeatures> features,
                List<List<LearningTactic?>> tactics) = GetDataForModel(sessions, defs);

            LogisticRegression model = new LogisticRegression(data);

            HashSet<int> defKeysPlotted = new HashSet<int>();

            List<LearningTactic> tacticsUsed = data.Select(d => d.session.GetType()).Distinct().Select(t => (LearningTactic)Activator.CreateInstance(t)).ToList();

            // just for debug

            foreach (DefinitionFeatures feature in features)
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

                ProbabilityPlotter.PlotProbabilityCurves(model, feature, tacticsUsed, $"plots/{language}/{name}.png");
            }

            Dictionary<int, DefinitionFeatures> defFeaturesLookup = new Dictionary<int, DefinitionFeatures>();
            Dictionary<int, Tuple<LearningTactic, DateTime>> lastTacticUsed = new Dictionary<int, Tuple<LearningTactic, DateTime>>();

            if (features.Count != tactics.Count)
            {
                throw new Exception("invalid data!"); // TODO data transfer class or something
            }

            for (int i = 0; i != features.Count; i++)
            {
                DefinitionFeatures fs = features[i];
                defFeaturesLookup.Add(fs.def.DatabasePrimaryKey, fs);

                for (int j = sessions.Count - 1; j >= 0; j--)
                {
                    LearningTactic? tactic = tactics[i][j];
                    if (tactic is not null)
                    {
                        DateTime when = sessions[j].First().Posed;

                        lastTacticUsed.Add(fs.def.DatabasePrimaryKey, Tuple.Create(tactic, when));

                        break;
                    }
                }

            }


            Model = model;
            DefFeatures = defFeaturesLookup.AsReadOnly();
            TacticsUsed = tacticsUsed.Select(lt => lt.GetType()).ToList();
            LastTacticUsed = lastTacticUsed.AsReadOnly();
        }

        private (List<FollowingSessionDatum>, List<DefinitionFeatures>, List<List<LearningTactic?>>) GetDataForModel(
            List<List<TestRecord>> sessions, List<DictionaryDefinition> defs)
        {
            DateTime now = DateTime.Now;

            List<DefinitionFeatures> features = new List<DefinitionFeatures>();
            List<List<LearningTactic?>> tactics = new List<List<LearningTactic?>>();

            foreach (DictionaryDefinition def in defs)
            {
                (DefinitionFeatures fs, List<LearningTactic?> ts) = GetFeaturesAndTactics(def, sessions, now);
                features.Add(fs);
                tactics.Add(ts);
            }

            List<FollowingSessionDatum> dataPoints = new List<FollowingSessionDatum>();

            List<DateTime> sessionTimes = sessions.Select(session => session.First().Posed).ToList();

            for (int i = 0; i != features.Count; i++)
            {
                dataPoints.AddRange(GenerateFollowingSessionData(features[i], tactics[i], sessionTimes));
            }

            return (dataPoints, features, tactics);
        }

        private List<FollowingSessionDatum> GenerateFollowingSessionData(
            DefinitionFeatures defFeatures, 
            List<LearningTactic?> defTactics, 
            List<DateTime> sessionTimes)
        {
            if (defTactics.Count != sessionTimes.Count)
            {
                throw new Exception("invalid data provided!");
            }

            List<FollowingSessionDatum> ret = new List<FollowingSessionDatum>();

            int next_id = -1;

            for (int i = defTactics.Count - 1; i >=0; i--)
            {
                if (next_id != -1 && defTactics[i] is not null)
                {
                    TimeSpan delta = sessionTimes[next_id] - sessionTimes[i];
                    double intervalDays = delta.TotalDays;
                    bool nextWasCorrect = WasCorrect(defTactics[next_id]);


                    ret.Add(new FollowingSessionDatum(
                        defFeatures,
                        defTactics[i],
                        intervalDays,
                        nextWasCorrect));
                }

                if (defTactics[i] is not null)
                {
                    next_id = i;
                }
            }
            return ret;
        }

        // then fit linear model of probability correct in one
        // of log time since last tactic
        // tactic type
        // features

        // do the same for eventually learnt



    }
}

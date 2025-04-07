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

namespace Learning
{


    public class FeatureVectoriser
    {
        private readonly Dictionary<Type, int> _tacticTypeToIndex;

        public FeatureVectoriser(IEnumerable<FollowingSessionDatum> data)
        {
            var tacticTypes = data.Select(d => d.session.GetType()).Distinct().ToList();
            _tacticTypeToIndex = tacticTypes.Select((type, i) => new { type, i })
                                            .ToDictionary(x => x.type, x => x.i);
        }
        public int FeatureCount => 7 + 1 + _tacticTypeToIndex.Count;

        public Vector<double> Vectorize(FollowingSessionDatum datum)
        {
            var features = new List<double>();

            var def = datum.defFeatures;
            features.Add(def.maxTimeBetweenCorrectDays);
            features.Add(def.sqrtTotalExposures);
            features.Add(def.fractionCorrect);
            features.Add(def.minTimeBetweenIncorrectDays);
            features.Add(def.avgTimeBetweenSessionsDays);
            features.Add(def.halfLifeDays);
            features.Add(1.0); // bias

            double intervalLogRatio = Math.Log(datum.intervalDays / (def.halfLifeDays + 1e-6));
            features.Add(intervalLogRatio);

            var oneHot = new double[_tacticTypeToIndex.Count];
            oneHot[_tacticTypeToIndex[datum.session.GetType()]] = 1;
            features.AddRange(oneHot);

            return Vector<double>.Build.DenseOfEnumerable(features);
        }

        public Vector<double> GetTarget(FollowingSessionDatum datum)
        {
            return Vector<double>.Build.Dense(new[] { datum.followingWasCorrect ? 1.0 : 0.0 });
        }
    }


    class Strategist
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

        #region features

        private (DefinitionFeatures, List<LearningTactic?>) GetFeaturesAndTactics(
            DictionaryDefinition   def,
            List<List<TestRecord>> sessions,
            DateTime at)
        {
            List<LearningTactic?> tactics = Tactics.IdentifyTacticsForSessions(sessions, def.DatabasePrimaryKey);

            double maxTimeBetweenCorrectDays = GetMaxTimeBetweenCorrect(
                sessions, tactics);

            double avgTimeBetweenSessionsDays;

            if (maxTimeBetweenCorrectDays == 0.0)
            {
                avgTimeBetweenSessionsDays = GetAvgTimeBetweenSessions(sessions, tactics);
            } 
            else
            {
                avgTimeBetweenSessionsDays = GetAvgTimeBetweenSessionsDecayed(
                    sessions, tactics, maxTimeBetweenCorrectDays, at);
            }

            double halfLife = maxTimeBetweenCorrectDays;

            if (halfLife == 0)
            {
                halfLife = avgTimeBetweenSessionsDays;
            }

            double minTimeBetweenIncorrectDays = GetMinTimeBetweenIncorrectDecayed(
                sessions, tactics, halfLife, at);

            double totalExposuresDecayed = GetTotalExposuresDecayed(
                def, sessions, halfLife, at);

            // best practice with count variables
            double totalExposuresSqrtDecayed = Math.Sqrt(totalExposuresDecayed);

            double fractionCorrect = GetFractionCorrectDecayed(
                def, sessions, halfLife, at);

            return (
                    new DefinitionFeatures(
                        def, 
                        maxTimeBetweenCorrectDays, 
                        totalExposuresSqrtDecayed, 
                        fractionCorrect, 
                        minTimeBetweenIncorrectDays, 
                        avgTimeBetweenSessionsDays,
                        halfLife),
                    tactics
                    );
        }

        private double GetFractionCorrectDecayed(DictionaryDefinition def, List<List<TestRecord>> sessions, double halfLife, DateTime at)
        {
            double toDiv = 0.0;
            double div   = 0.0;

            foreach (List<TestRecord> session in sessions)
            {
                int total = session.Where(tr => tr.DictionaryDefinitionKey == def.DatabasePrimaryKey).Count();
                if (total == 0)
                {
                    continue;
                }
                int correct = session.Where(tr => tr.DictionaryDefinitionKey == def.DatabasePrimaryKey).Where(tr => tr.Correct == true).Count();

                DateTime time = session.First().Posed;
                TimeSpan ago = at - time;

                double decay = Math.Pow(0.5, ago.TotalDays);

                toDiv += (correct / total) * decay;
                div   += decay;
            }

            if (div == 0.0)
            {
                return 0.5;
            } 
            else
            {
                return toDiv / div;
            }
        }

        private double GetTotalExposuresDecayed(DictionaryDefinition def, List<List<TestRecord>> sessions, double halfLife, DateTime at)
        {
            double ret = 0.0;

            foreach (List<TestRecord> session in sessions)
            {
                DateTime time = session.First().Posed;
                TimeSpan ago = at - time;

                double decay = Math.Pow(0.5, ago.TotalDays);

                ret += session.Where(tr => tr.DictionaryDefinitionKey == def.DatabasePrimaryKey).Count() * decay;
            }

            return ret;
        }

        private double GetMinTimeBetweenIncorrectDecayed(List<List<TestRecord>> sessions, List<LearningTactic?> tactics, double halfLife, DateTime at)
        {
            double ret = double.MaxValue;
            bool goodRet = false;

            int last_i = -1;

            for (int this_i = 0; this_i != tactics.Count; this_i++)
            {
                if (last_i != -1 && tactics[this_i] is not null)
                {
                    bool thisInCorrect = !WasCorrect(tactics[this_i]);

                    if (thisInCorrect)
                    {
                        DateTime lastTime = sessions[last_i].First().Posed;
                        DateTime thisTime = sessions[this_i].First().Posed;
                        TimeSpan delta = thisTime - lastTime;

                        DateTime mid = lastTime + delta / 2.0;
                        TimeSpan daysSinceMid = at - mid;

                        double decay = Math.Pow(0.5, daysSinceMid.TotalDays);

                        delta /= decay; // since taking min, we inflate times longer ago

                        ret = Math.Min(ret, delta.TotalDays);
                        goodRet = true;
                    }
                }

                if (tactics[this_i] is not null)
                {
                    last_i = this_i;
                }
            }

            if (goodRet)
            {
                return ret;
            } else
            {
                return 1.0;
            }
        }

        private bool WasCorrect(LearningTactic tactic)
        {
            if (tactic.Prerequisite is null)
            {
                return false;
            }
            if (tactic.Prerequisite.GetType() == typeof(AllCorrect))
            {
                return true;
            }

            return WasCorrect(tactic.Prerequisite);
        }

        private double GetMaxTimeBetweenCorrect(List<List<TestRecord>> sessions, List<LearningTactic?> tactics)
        {
            double maxTimeBetweenCorrectDays = 0.0;

            int last_i = -1;

            for (int this_i = 0; this_i != tactics.Count; this_i++)
            {
                if (last_i != -1 && tactics[this_i] is not null)
                {
                    bool lastCorrect = WasCorrect(tactics[last_i]);
                    bool thisCorrect = WasCorrect(tactics[this_i]);

                    if (lastCorrect && thisCorrect)
                    {
                        DateTime lastTime = sessions[last_i].First().Posed;
                        DateTime thisTime = sessions[this_i].First().Posed;
                        TimeSpan delta = thisTime - lastTime;

                        maxTimeBetweenCorrectDays = Math.Max(
                            maxTimeBetweenCorrectDays,
                            delta.TotalDays);
                    }
                }

                if (tactics[this_i] is not null)
                {
                    last_i = this_i;
                }
            }

            return maxTimeBetweenCorrectDays;
        }
        
        private double GetAvgTimeBetweenSessions(List<List<TestRecord>> sessions, List<LearningTactic?> tactics)
        {
            TimeSpan total = TimeSpan.FromTicks(0);
            int div = 0;

            int last_i = -1;

            for (int this_i = 0; this_i != tactics.Count; this_i++)
            {
                if (last_i != -1 && tactics[this_i] is not null)
                {
                    DateTime lastTime = sessions[last_i].First().Posed;
                    DateTime thisTime = sessions[this_i].First().Posed;
                    TimeSpan delta = thisTime - lastTime;

                    total += delta; div++;
                }

                if (tactics[this_i] is not null)
                {
                    last_i = this_i;
                }
            }

            if (div != 0)
            {
                return total.TotalDays / div;
            }
            else
            {
                return 1.0;
            }
        }
        
        private double GetAvgTimeBetweenSessionsDecayed(List<List<TestRecord>> sessions, List<LearningTactic?> tactics, double halfLifeDays, DateTime at)
        {
            TimeSpan total = TimeSpan.FromTicks(0);
            double div = 0.0;

            int last_i = -1;

            for (int this_i = 0; this_i != tactics.Count; this_i++)
            {
                if (last_i != -1 && tactics[this_i] is not null)
                {
                    DateTime lastTime = sessions[last_i].First().Posed;
                    DateTime thisTime = sessions[this_i].First().Posed;
                    TimeSpan delta = thisTime - lastTime;

                    DateTime mid = lastTime + delta / 2.0;
                    TimeSpan daysSinceMid = at - mid;

                    // time weighted average
                    double decay = Math.Pow(0.5, daysSinceMid.TotalDays);

                    delta *= decay;

                    total += delta; 
                    div += decay;
                }

                if (tactics[this_i] is not null)
                {
                    last_i = this_i;
                }
            }

            if (div != 0)
            {
                return total.TotalDays / div;
            }
            else
            {
                return 1.0;
            }
        }

        #endregion

        public void BuildModel(List<List<TestRecord>> sessions, List<DictionaryDefinition> defs)
        {
            (List<FollowingSessionDatum> data, List<DefinitionFeatures> features) = GetDataForModel(sessions, defs);

            LogisticRegression model = new LogisticRegression(data);

            HashSet<int> defKeysPlotted = new HashSet<int>();

            List<LearningTactic> tactics = data.Select(d => d.session.GetType()).Distinct().Select(t => (LearningTactic)Activator.CreateInstance(t)).ToList();

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

                ProbabilityPlotter.PlotProbabilityCurves(model, feature, tactics, $"plots/{language}/{name}.png");
            }


        }

        private (List<FollowingSessionDatum>, List<DefinitionFeatures>) GetDataForModel(
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

            return (dataPoints, features);
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

using DataClasses;
using Learning.Strategy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning
{
    partial class Strategist
    {
        private ModelData GetDataForModel(
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

            List<Type> tacticsUsed = dataPoints.Select(d => d.sessionTacticType).Distinct().ToList();

            Dictionary<int, DefinitionFeatures> defFeaturesLookup = new Dictionary<int, DefinitionFeatures>();
            Dictionary<int, Tuple<LearningTactic, DateTime>> lastTacticUsed = new Dictionary<int, Tuple<LearningTactic, DateTime>>();

            if (features.Count != tactics.Count)
            {
                throw new Exception("invalid data!");
            }

            for (int def_idx = 0; def_idx != features.Count; def_idx++)
            {
                DefinitionFeatures fs = features[def_idx];
                defFeaturesLookup.Add(fs.def.DatabasePrimaryKey, fs);

                for (int sesh_idx = sessions.Count - 1; sesh_idx >= 0; sesh_idx--)
                {
                    LearningTactic? tactic = tactics[def_idx][sesh_idx];
                    if (tactic is not null)
                    {
                        DateTime when = sessions[sesh_idx].First().Posed;

                        lastTacticUsed.Add(fs.def.DatabasePrimaryKey, Tuple.Create(tactic, when));

                        break;
                    }
                }

            }

            return new ModelData(
                dataPoints,
                features,
                tactics,
                sessions,
                tacticsUsed,
                lastTacticUsed,
                defFeaturesLookup);
        }

        private static List<FollowingSessionDatum> GenerateFollowingSessionData(
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

            for (int i = defTactics.Count - 1; i >= 0; i--)
            {
                if (next_id != -1 && defTactics[i] is not null)
                {
                    TimeSpan delta = sessionTimes[next_id] - sessionTimes[i];
                    double intervalDays = delta.TotalDays;
                    bool nextWasCorrect = WasCorrect(defTactics[next_id]);

                    ret.Add(new FollowingSessionDatum(
                        defFeatures,
                        defTactics[i].GetType(),
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

        public static FollowingSessionDatum CreateDatum(DefinitionFeatures features, Type tacticType, double interval)
        {
            return new FollowingSessionDatum(
                defFeatures: features,
                sessionTacticType: tacticType,
                intervalDays: interval,
                followingWasCorrect: false // prediction ignores this
            );
        }
    }
}

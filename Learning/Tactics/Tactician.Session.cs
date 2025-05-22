using DataClasses;
using Serilog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning
{
    partial class Tactician
    {
        private List<TestRecord>?        CurrentSession       { get; set; }
        private Dictionary<int, Type>?   CurrentTacticalState { get; set; } = null;
        private ConcurrentDictionary<int, double>? CurrentTwistScores   { get; set; } = null; // "stick or twist"


        Dictionary<int, int> CoolOff    { get; set; } = new Dictionary<int, int>();
        Dictionary<int, int> CoolOffMax { get; set; } = new Dictionary<int, int>();


        private double Temperature { get; set; } = 0.03; // P(sample a low data volume tactic definition)


        private HashSet<int> Ignored { get; init; } = new HashSet<int>();

        internal int GetBestDefID()
        {
            var random = new Random();

            double temp = random.NextDouble();

            if (temp < Temperature)
            {
                return InfrequentlySeenTacticDefinition(temp);
            }

            var candidates = CurrentTwistScores
                .Where(kv => !CoolOff.ContainsKey(kv.Key))
                .Where(kv => !Ignored.Contains(kv.Key));

            var maxScore = candidates.Max(kv => kv.Value);

            var topCandidates = candidates
                .Where(kv => kv.Value == maxScore)
                .ToList();
            
            int ret = topCandidates[random.Next(topCandidates.Count)].Key;
            Log.Information("found id with twist score of {value}", CurrentTwistScores[ret]);
            Log.Information("total occurences: {occurences}", Strategist.VocabModel.WordFrequencies[ret]);

            if (CurrentTacticalState?.ContainsKey(ret) ?? false)
            {
                Log.Information("was in state {state}", CurrentTacticalState[ret].Name.Split('.').Last());
            }

            return ret;
        }

        private int InfrequentlySeenTacticDefinition(double stop)
        {
            if (stop > 1.0)
            {
                throw new ArgumentException("stop must be in [0,1]");
            }

            Log.Information("getting infrequent definition with temperature {temp}", stop);

            // the low datavolume models are pretty bad - so we make sure to pick out samples from them
            // regardless from time to time, so hopefully over time they will improve

            var loToHi = Strategist.ModelData.tacticProportions.OrderBy(kvp => kvp.Value).ToList(); ;

            double cum = 0;
            int index = -1;
            Type toTest = loToHi.Last().Key;

            // use this accumulation method to keep the return tactic roughly "on distribution"
            // since we don't want to always return the very rare tactics, since then modelling them becomes a bit pointless
            // since this method would dominate the serving of those definitions, not the statistical model!

            while (index < loToHi.Count)
            {
                index++;

                cum += loToHi[index].Value;

                if (cum > stop)
                {
                    toTest = loToHi[index].Key; break;
                }
            }

            Log.Information("want to test {totest}", toTest.Name);

            List<KeyValuePair<int, Tuple<LearningTactic, DateTime>>> ofThisTactic = Strategist.ModelData.distinctDefinitionsLastTacticUsed
                .Where(kvp => kvp.Value.Item1.GetType() == toTest)
                .ToList();
            
            if (ofThisTactic.Count == 0)
            {

                Log.Information("found no examples");

                while (true)
                {
                    index++;

                    toTest = loToHi[index].Key;

                    ofThisTactic = Strategist.ModelData.distinctDefinitionsLastTacticUsed
                        .Where(kvp => kvp.Value.Item1.GetType() == toTest)
                        .ToList();

                    if (ofThisTactic.Count > 0) { break; }

                    Log.Information("found no examples");
                }
            }

            Log.Information("testing {testingName}", toTest.Name);

            Random rng = new Random();
            var shuffled = ofThisTactic.OrderBy(_ => rng.Next()).ToList();

            return shuffled.First().Key;
        }

        internal void Inform(TestRecord tr)
        {
            if (CurrentSession is null || tr.Posed - CurrentSession.Last().Finished > LearningTacticsHelper.MinTimeBetweenSessions)
            {
                StartNewSession(tr);
            }
            else
            {
                CurrentSession.Add(tr);
            }

            UpdateCoolOffs(tr.DictionaryDefinitionKey, tr.Correct);

            UpdateTacticalState();
        }


        internal void Ignore(int defKey)
        {
            Ignored.Add(defKey);
        }
        private void UpdateCoolOffs(int defKey, bool correct)
        {

            if (correct)
            {
                if (!CoolOffMax.ContainsKey(defKey))
                {
                    CoolOff[defKey]    = 0;
                    CoolOffMax[defKey] = 1;
                }

                CoolOffMax[defKey]++; CoolOffMax[defKey]++;
                CoolOff[defKey] = CoolOffMax[defKey];
            }
            else
            {
                CoolOff.Remove(defKey);
                CoolOffMax[defKey] = 0;
            }

            foreach (var l in CoolOff.Keys)
            {
                CoolOff[l]--;
                if (CoolOff[l] <= 0)
                {
                    CoolOff.Remove(l);
                }
            }
        }

        private void StartNewSession(TestRecord first)
        {
            CurrentSession       = new List<TestRecord> { first };
            CurrentTacticalState = new Dictionary<int, Type>();

            CoolOffMax = new Dictionary<int, int>();
            CoolOff    = new Dictionary<int, int>();
        }

        private void UpdateTacticalState()
        {
            // now just use the last def id
            // in the future if we have cooloff tactics, then this will need to be more advanced

            int lastDefTestedKey = CurrentSession.Last().DictionaryDefinitionKey;

            Type? tacticType = Strategist.TacticsHelper.IdentityTacticForSession(CurrentSession, lastDefTestedKey)?.GetType() ?? null;

            if (tacticType is null)
            {
                throw new Exception("couldn't identify a tactic for a tested definition");
            }

            CurrentTacticalState[lastDefTestedKey] = tacticType;

            Log.Information("entered new state {tactic}", tacticType);

            UpdateTwistScores(lastDefTestedKey);
        }
    }
}

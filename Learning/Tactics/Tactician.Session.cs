using DataClasses;
using Serilog;
using System;
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
        private Dictionary<int, double>? CurrentTwistScores   { get; set; } = null; // "stick or twist"


        Dictionary<int, int> CoolOff    { get; set; } = new Dictionary<int, int>();
        Dictionary<int, int> CoolOffMax { get; set; } = new Dictionary<int, int>();

        internal int GetBestDefID()
        {
            var candidates = CurrentTwistScores
                .Where(kv => !CoolOff.ContainsKey(kv.Key));

            var maxScore = candidates.Max(kv => kv.Value);

            var topCandidates = candidates
                .Where(kv => kv.Value == maxScore)
                .ToList();

            var random = new Random();
            int ret = topCandidates[random.Next(topCandidates.Count)].Key;
            Log.Information("found id with twist score of {value}", CurrentTwistScores[ret]);
            Log.Information("total occurences: {occurences}", Strategist.VocabModel.WordFrequencies[ret]);

            if (CurrentTacticalState?.ContainsKey(ret) ?? false)
            {
                Log.Information("was in state {state}", CurrentTacticalState[ret].Name.Split('.').Last());
            }

            return ret;
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

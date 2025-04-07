using DataClasses;
using Learning.LearningTacticsRepository;
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
        private (DefinitionFeatures, List<LearningTactic?>) GetFeaturesAndTactics(
            DictionaryDefinition def,
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
            double div = 0.0;

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
                div += decay;
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
            }
            else
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
    }
}

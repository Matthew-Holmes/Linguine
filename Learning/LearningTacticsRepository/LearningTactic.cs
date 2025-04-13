using DataClasses;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.Statistics;

namespace Learning
{
    internal record BasicThresholds(
        int? MinExposures = null, int? MaxExposures = null, 
        int? MinCorrect   = null, int? MaxCorrect   = null,
        int? MinIncorrect = null, int? MaxIncorrect = null,
        TimeSpan? MinTotalTime   = null, TimeSpan? MaxTotalTime   = null, 
        TimeSpan? MinAverageTime = null, TimeSpan? MaxAverageTime = null);


    internal delegate bool Constraint(List<TestRecord> sessionRecords, int DefID);

    internal abstract class LearningTactic
    {
        // TODO - while this is suitable for identifying tactics
        // need more thought when it comes to employing them
        // TODO - this makes a tree sort of structure - is that sufficient?
        internal abstract LearningTactic? Prerequisite { get; }

        internal virtual List<BasicThresholds> Thresholds { get; init; } = new List<BasicThresholds>();

        protected virtual List<Constraint> Constraints { get; init; } = new List<Constraint>();


        internal bool UsedThisTactic(List<TestRecord> sessionRecords, int defID)
        {
            if (Prerequisite?.UsedThisTactic(sessionRecords, defID) ?? true)
            {
                /* do nothing */
            } else
            {
                return false;
            }

            // must satisfy every constraint

            foreach (Constraint constraint in Constraints)
            {
                if (!constraint(sessionRecords, defID))
                {
                    return false;
                }
            }


            foreach (BasicThresholds threshold in Thresholds)
            {
                if (!IsSatisfied(threshold, sessionRecords, defID))
                {
                    return false;
                }
            }

            return true;
        }


        private TimeSpan TimeClip => TimeSpan.FromMinutes(1); // edge cases that saw very long times

        private bool IsSatisfied(BasicThresholds constraint, List<TestRecord> sessionRecords, int defID)
        {
            List<TestRecord> forThisDef = sessionRecords.Where(tr => tr.DictionaryDefinitionKey == defID).ToList();

            if (forThisDef.Count == 0)
            {
                return false; // no records = no tactic! (or do nothing tactic??)
            }

            int totalExposures = forThisDef.Count();
            int totalCorrect   = forThisDef.Where(tr => tr.Correct == true).Count();
            int totalIncorrect = totalExposures - totalCorrect;

            TimeSpan totalTime = TimeSpan.FromTicks(
                forThisDef.Select(tr => tr.Finished - tr.Posed)
                          .Select(ts => ts.Ticks)
                          .Select(ticks => Math.Min(ticks, TimeClip.Ticks))
                          .Sum());

            // use media to avoid outliers
            TimeSpan averageTime = TimeSpan.FromMinutes(
                forThisDef.Select(tr => tr.Finished - tr.Posed)
                .Select(ts => ts.TotalMinutes)
                .Median());

            bool satisfiesExposures = true;

            if (constraint.MaxExposures is not null && totalExposures > constraint.MaxExposures)
            {
                satisfiesExposures = false;
            }
            if (constraint.MinExposures is not null && totalExposures < constraint.MinExposures)
            {
                satisfiesExposures = false;
            }

            bool satisfiesCorrect = true;

            if (constraint.MaxCorrect is not null && totalCorrect > constraint.MaxCorrect)
            {
                satisfiesCorrect = false;
            }
            if (constraint.MinCorrect is not null && totalCorrect < constraint.MinCorrect)
            {
                satisfiesCorrect = false;
            }

            bool satisfiesIncorrect = true;

            if (constraint.MaxIncorrect is not null && totalIncorrect > constraint.MaxIncorrect)
            {
                satisfiesIncorrect = false;
            }
            if (constraint.MinIncorrect is not null && totalIncorrect < constraint.MinIncorrect)
            {
                satisfiesIncorrect = false;
            }

            bool satisfiesTotalTime = true;

            if (constraint.MaxTotalTime is not null && totalTime > constraint.MaxTotalTime)
            {
                satisfiesTotalTime = false;
            }
            if (constraint.MinTotalTime is not null && totalTime < constraint.MinTotalTime)
            {
                satisfiesTotalTime = false;
            }

            bool satisfiesAvgTime = true;

            if (constraint.MaxAverageTime is not null && averageTime > constraint.MaxAverageTime)
            {
                satisfiesAvgTime = false;
            }
            if (constraint.MinAverageTime is not null && averageTime < constraint.MinAverageTime)
            {
                satisfiesAvgTime = false;
            }


            return satisfiesExposures && satisfiesCorrect && satisfiesIncorrect && satisfiesTotalTime && satisfiesAvgTime;
        }
    }
}

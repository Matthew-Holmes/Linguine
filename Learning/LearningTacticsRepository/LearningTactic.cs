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
    public record BasicThresholds(
        int? MinExposures = null, int? MaxExposures = null, 
        int? MinCorrect   = null, int? MaxCorrect   = null,
        int? MinIncorrect = null, int? MaxIncorrect = null,
        TimeSpan? MinTotalTime   = null, TimeSpan? MaxTotalTime   = null, 
        TimeSpan? MinAverageTime = null, TimeSpan? MaxAverageTime = null);


    public delegate bool Constraint(List<TestRecord> sessionRecords, int DefID);

    abstract class LearningTactic
    {
        // TODO - while this is suitable for identifying tactics
        // need more thought when it comes to employing them
        // TODO - this makes a tree sort of structure - is that sufficient?
        public abstract LearningTactic? Prerequisite { get; }

        public virtual List<BasicThresholds> NecThresholds { get; init; } = new List<BasicThresholds>();
        public virtual List<BasicThresholds> SufThresholds { get; init; } = new List<BasicThresholds>();


        protected virtual List<Constraint> SufConstraints { get; init; } = new List<Constraint>();
        protected virtual List<Constraint> NecConstraints { get; init; } = new List<Constraint>();


        public bool UsedThisTactic(List<TestRecord> sessionRecords, int defID)
        {
            if (Prerequisite?.UsedThisTactic(sessionRecords, defID) ?? true)
            {
                /* do nothing */
            } else
            {
                return false;
            }

            #region basic thresholds
            bool metNecThresholds = true;

            foreach (BasicThresholds threshold in NecThresholds)
            {
                metNecThresholds = metNecThresholds && IsSatisfied(threshold, sessionRecords, defID);
            }

            bool metSufThresholds = false;

            foreach (BasicThresholds threshold in SufThresholds)
            {
                metSufThresholds = metSufThresholds || IsSatisfied(threshold, sessionRecords, defID);
            }
            #endregion
            if (metSufThresholds && !metNecThresholds)
            {
                Log.Error("logical inconsistency in basic thresholds");
                throw new Exception("invalid sufficient and necessary basic thresholds!");
            }
            bool metBasicThresholds = metNecThresholds || metSufThresholds;

            #region constraints
            bool metNecConstraints = true;
            foreach (Constraint constraint in NecConstraints)
            {
                metNecConstraints = metNecConstraints && constraint(sessionRecords, defID);
            }

            bool metSufConstraints = false;
            foreach (Constraint constraint in SufConstraints)
            {
                metSufConstraints = metSufConstraints || constraint(sessionRecords, defID);
            }
            #endregion

            if (metSufConstraints && !metNecConstraints)
            {
                Log.Error("logical inconsistency in constraints!");
                throw new Exception("logical inconsistency in constraints!");
            }
            bool metConstrains = metNecConstraints || metSufConstraints;

            return metBasicThresholds && metConstrains;
        }


        private TimeSpan TimeClip => TimeSpan.FromMinutes(1); // edge cases that saw very long times

        private bool IsSatisfied(BasicThresholds constraint, List<TestRecord> sessionRecords, int defID)
        {
            List<TestRecord> forThisDef = sessionRecords.Where(tr => tr.DatabasePrimaryKey == defID).ToList();

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
                          .Select(ticks => Math.Max(ticks, TimeClip.Ticks))
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

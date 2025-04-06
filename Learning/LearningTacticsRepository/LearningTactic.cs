using DataClasses;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning
{
    public record BasicThresholds(
        int? MinExposures, int? MaxExposures, 
        int? MinCorrect,   int? MaxCorrect,
        int? MinIncorrect, int? MaxIncorrect,
        TimeSpan? MinTotalTime,   TimeSpan? MaxTotalTime, 
        TimeSpan? MinAverageTime, TimeSpan? MaxAverageTime);


    public delegate bool Constraint(List<TestRecord> sessionRecords, int DefID);

    abstract class LearningTactic
    {
        // TODO - while these are suitable for identifying tactics
        // need more thought when it comes to employing them
        public abstract List<LearningTactic> Prerequisites { get; init; }

        public abstract List<BasicThresholds> NecThresholds { get; init; }
        public abstract List<BasicThresholds> SufThresholds { get; init; }


        protected abstract List<Constraint> SufConstraints { get; init; }
        protected abstract List<Constraint> NecConstraints { get; init; }


        public bool UsedThisTactic(List<TestRecord> sessionRecords, int defID)
        {
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
                           .Select(ts => ts.Ticks).Sum());

            TimeSpan averageTime = totalTime / totalExposures;

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

using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.Strategy
{
    public record DefinitionFeatures(DictionaryDefinition def,
                                     double maxTimeBetweenCorrectDays,
                                     double sqrtTotalExposures,
                                     double fractionCorrect,
                                     double minTimeBetweenIncorrectDays,
                                     double avgTimeBetweenSessionsDays,
                                     double halfLifeDays);

    public record FollowingSessionDatum(
        DefinitionFeatures defFeatures,
        LearningTactic session,
        double intervalDays,
        bool followingWasCorrect);
}

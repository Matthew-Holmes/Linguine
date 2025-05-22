using DataClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Learning.Strategy
{
    internal record DefinitionFeatures(
        DictionaryDefinition def,
        double maxTimeBetweenCorrectDays,
        double sqrtTotalExposures,
        double fractionCorrect,
        double minTimeBetweenIncorrectDays,
        double avgTimeBetweenSessionsDays,
        double halfLifeDays,
        double zipfScore);

    internal record FollowingSessionDatum(
        DefinitionFeatures defFeatures,
        Type sessionTacticType,
        double intervalDays,
        bool followingWasCorrect);

    internal record ModelData(
        List<FollowingSessionDatum> trainingData,
        List<DefinitionFeatures> distinctDefinitionFeatures,
        List<List<LearningTactic?>> distinctDefinitionTacticsIdentified,
        List<List<TestRecord>> sessions,
        List<Type> tacticsUsed,
        Dictionary<int, Tuple<LearningTactic, DateTime>> distinctDefinitionsLastTacticUsed,
        Dictionary<int, DefinitionFeatures> defFeaturesLookup,
        Dictionary<Type, double> followingSessionAverages,
        double tacticAverageReward,
        Dictionary<Type, double> tacticProportions);
}

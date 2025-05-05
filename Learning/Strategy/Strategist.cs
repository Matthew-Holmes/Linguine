using DataClasses;
using Learning.Strategy;
using Serilog;

namespace Learning
{
    using DefFeatureLookup     = IReadOnlyDictionary<int, DefinitionFeatures>;
    using LastTacticInfoLookup = IReadOnlyDictionary<int, Tuple<LearningTactic, DateTime>>;
    using RewardLookup         = IReadOnlyDictionary<Type, double>;

    internal partial class Strategist
    {
        // Agent that aims to maximise the probability of correct response on first try
        // using discounted rewards looking into the future

        // sees a history of past learning tactics and models this probability into the future
        // do this via a latent embedding of the word, with the latent embedding produced by the history

        // then will be presented with variety of options by the Tactician
        // and choose the one with highest increase in future reward

        // most of the model should be based off of the last session
        // since this is what we can control when planning the next session

        internal DefFeatureLookup     DefFeatures    { get; private set; }
        internal LastTacticInfoLookup LastTacticUsed { get; private set; }
        internal RewardLookup         DefaultRewards { get; private set; }


        internal LearningTacticsHelper TacticsHelper = new LearningTacticsHelper();
        internal List<Type> TacticsUsed { get; private set; }

        internal double BaseLineReward        { get; private set; }
        internal double BaseLineInitialPKnown { get; private set; }

        // telemetry/debug properties
        internal LogisticRegression Model     { get; set; }
        internal VocabularyModel    VocabModel { get; init; }
        internal ModelData          ModelData { get; set; }


        internal Strategist(VocabularyModel vocab, List<List<TestRecord>> sessions, List<DictionaryDefinition> defs, CancellationTokenSource cts)
        {
            VocabModel = vocab; // set this before generating model data

            ModelData modelData = GetDataForModel(sessions, defs);

            ModelData      = modelData;
            TacticsUsed    = modelData.tacticsUsed;
            DefFeatures    = modelData.defFeaturesLookup.AsReadOnly();
            LastTacticUsed = modelData.distinctDefinitionsLastTacticUsed.AsReadOnly();
            DefaultRewards = modelData.followingSessionAverages.AsReadOnly(); ;
            BaseLineReward = modelData.tacticAverageReward;

            Model = new LogisticRegression(
                            modelData.trainingData, modelData.tacticsUsed);
        }

        internal double PredictProbability(FollowingSessionDatum input)
        {
            return Model.PredictProbability(input, TacticsUsed);
        }

        internal double GetExistingPKnown(int defKey, double lookAheadDays)
        {
            // predicts the probability we will know the definition in one day
            // todo - use discounted return?

            FollowingSessionDatum? input = GetCurrentRewardFeatures(defKey, lookAheadDays);

            if (input is null)
            {
                return VocabModel.PKnownWithError[defKey].Item1;
            } else
            {
                return PredictProbability(input);
            }
        }

        internal FollowingSessionDatum? GetCurrentRewardFeatures(int key, double lookAheadDays)
        {
            if (!LastTacticUsed.ContainsKey(key))
            {
                return null;
            }

            if (LastTacticUsed[key] is null)
            {
                return null;
            }

            LearningTactic lastTactic = LastTacticUsed[key].Item1;
            DateTime when = LastTacticUsed[key].Item2;

            Type lastTacticType = lastTactic.GetType();

            if (!TacticsUsed.Contains(lastTacticType))
            {
                Log.Warning("encountered last tactic type that was not logged as used");
                return null;
            }

            TimeSpan interval = DateTime.Now - when;
            double intervalDays = interval.TotalDays + lookAheadDays;

            DefinitionFeatures features = DefFeatures[key];

            return CreateDatum(features, lastTacticType, intervalDays);
        }
    }
}

using System;
using System.Collections.Generic;

namespace Infrastructure
{
    public record Config
    {
        public APIKeysConfig                    APIKeys            { get; set; } = new();
        public LanguageConfig                   Languages          { get; set; } = new();

        public GimmicksConfig                   Gimmicks           { get; set; } = new();
        public String                           DatabaseDirectory  { get; }      = "Store";
        public String                           FilestoreDirectory { get; }      = "Filestore";

        public IReadOnlyDictionary<LanguageCode, string> ConnectionStrings { get; set; } = new Dictionary<LanguageCode, string>();

        public LearnerLevel GetLearnerLevel()
        {
            return Languages.LearnerLevels[Languages.TargetLanguage];
        }

        public String GetDatabaseString()
        {
            return ConnectionStrings[Languages.TargetLanguage];
        }

        public bool LearningForeignLanguage()
        {
            return Languages.TargetLanguage != Languages.NativeLanguage;
        }

    }

    public record APIKeysConfig
    {
        public string OpenAI_APIKeyLocation   { get; set; } = string.Empty;
        public string DeepSeek_APIKeyLocation { get; set; } = string.Empty;

        public string APIKeyDirectory         { get; }      = "Filestore/APIKeys";
    }

    public record LanguageConfig
    {
        public LanguageCode                           NativeLanguage { get; set; }
        public LanguageCode                           TargetLanguage { get; set; }
        public IReadOnlyDictionary<LanguageCode, LearnerLevel> LearnerLevels  { get; set; } = new Dictionary<LanguageCode, LearnerLevel>();
    }

    public record GimmicksConfig
    {
        public IReadOnlyDictionary<LanguageCode, decimal> TimeToProcessSeconds { get; set; } = new Dictionary<LanguageCode, decimal>();
        public IReadOnlyDictionary<LanguageCode, int> CharsProcessedPerStep { get; set; } = new Dictionary<LanguageCode, int>();
    }

}

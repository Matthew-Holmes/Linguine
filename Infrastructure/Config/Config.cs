using System;
using System.Collections.Generic;

namespace Infrastructure
{
    public class Config
    {
        public APIKeysConfig                    APIKeys            { get; set; } = new();
        public LanguageConfig                   Languages          { get; set; } = new();
        public Dictionary<LanguageCode, string> ConnectionStrings  { get; set; } = new();

        public String                           DatabaseDirectory  { get; }      = "Store";
        public String                           FilestoreDirectory { get; }      = "Filestore";

        public LearnerLevel GetLearnerLevel()
        {
            return Languages.LearnerLevels[Languages.TargetLanguage];
        }

        public void SetLearnerLevel(LearnerLevel ll)
        {
            Languages.LearnerLevels[Languages.TargetLanguage] = ll;
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

    public class APIKeysConfig
    {
        public string OpenAI_APIKeyLocation   { get; set; } = string.Empty;
        public string DeepSeek_APIKeyLocation { get; set; } = string.Empty;

        public string APIKeyDirectory         { get; }      = "Filestore/APIKeys";
    }

    public class LanguageConfig
    {
        public LanguageCode                           NativeLanguage { get; set; }
        public LanguageCode                           TargetLanguage { get; set; }
        public Dictionary<LanguageCode, LearnerLevel> LearnerLevels  { get; set; } = new();
    }
}

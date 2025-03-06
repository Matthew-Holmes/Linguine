using System;
using System.Collections.Generic;

namespace Infrastructure
{
    public class Config
    {
        public APIKeysConfig                    APIKeys           { get; set; } = new();
        public LanguageConfig                   Languages         { get; set; } = new();
        public Dictionary<LanguageCode, string> ConnectionStrings { get; set; } = new();
    }

    public class APIKeysConfig
    {
        public string OpenAI_APIKeyLocation   { get; set; } = string.Empty;
        public string DeepSeek_APIKeyLocation { get; set; } = string.Empty;
    }

    public class LanguageConfig
    {
        public LanguageCode                           NativeLanguage { get; set; }
        public LanguageCode                           TargetLanguage { get; set; }
        public Dictionary<LanguageCode, LearnerLevel> LearnerLevels  { get; set; } = new();
    }
}

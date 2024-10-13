using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{


    [JsonObject(MemberSerialization.Fields)]
    internal class Config // keep internal: should not be able to instantiate in wider code
    {
        internal string OpenAI_APIKeyLocation;

        internal Dictionary<LanguageCode, String> ConnectionStrings = new Dictionary<LanguageCode, string>();

        [JsonConverter(typeof(StringEnumConverter))]
        internal LanguageCode NativeLanguage;
        [JsonConverter(typeof(StringEnumConverter))]
        internal LanguageCode TargetLanguage;


        internal Dictionary<LanguageCode, LearnerLevel> LearnerLevels =
            new Dictionary<LanguageCode, LearnerLevel>();

        internal Config Copy()
        {
            Config copy = new Config
            {
                NativeLanguage = this.NativeLanguage,
                TargetLanguage = this.TargetLanguage,

                OpenAI_APIKeyLocation = this.OpenAI_APIKeyLocation,

                ConnectionStrings = new Dictionary<LanguageCode, String>(),
                LearnerLevels = new Dictionary<LanguageCode, LearnerLevel>()

            };

            if (this.ConnectionStrings is not null)
            {
                foreach (var entry in this.ConnectionStrings)
                {
                    copy.ConnectionStrings.Add(entry.Key, entry.Value);
                }
            }

            if (this.LearnerLevels is not null)
            {
                foreach (var entry in this.LearnerLevels)
                {
                    copy.LearnerLevels.Add(entry.Key, entry.Value);
                }
            }

            return copy;
        }

        internal bool Equals(Config rhs)
        {
            if (rhs == null)
            {
                return false;
            }

            bool sameTarget = TargetLanguage == rhs.TargetLanguage;
            bool sameNative = NativeLanguage == rhs.NativeLanguage;

            bool sameAPIKeyLocation = OpenAI_APIKeyLocation == rhs.OpenAI_APIKeyLocation;

            bool sameConnectionStrings = ConnectionStrings.Keys.Count == rhs.ConnectionStrings.Keys.Count;

            foreach (var entry in this.ConnectionStrings)
            {
                sameConnectionStrings = sameConnectionStrings && (ConnectionStrings[entry.Key] == rhs.ConnectionStrings[entry.Key]);
            }

            bool sameLevels = LearnerLevels.Keys.Count == rhs.LearnerLevels.Keys.Count;

            foreach (var entry in this.LearnerLevels)
            {
                sameLevels = sameLevels && (LearnerLevels[entry.Key] ==
                    rhs.LearnerLevels[entry.Key]);
            }

            return sameTarget && sameNative && sameLevels && sameAPIKeyLocation && sameConnectionStrings;

        }

    }
}

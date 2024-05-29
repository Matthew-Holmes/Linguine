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

        internal Config Copy()
        {
            Config copy = new Config
            {
                NativeLanguage = this.NativeLanguage,
                TargetLanguage = this.TargetLanguage,

                OpenAI_APIKeyLocation = this.OpenAI_APIKeyLocation,

                ConnectionStrings = new Dictionary<LanguageCode, String>()

            };

            if (this.ConnectionStrings is not null)
            {
                foreach (var entry in this.ConnectionStrings)
                {
                    copy.ConnectionStrings.Add(entry.Key, entry.Value);
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

            return sameTarget && sameNative && sameAPIKeyLocation && sameConnectionStrings;

        }

    }
}

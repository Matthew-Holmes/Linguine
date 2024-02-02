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
        internal string FileStoreLocation;
        internal string DictionariesDirectory;
        internal string VariantsDirectory;

        internal Dictionary<LanguageCode, List<Tuple<String, String>>> SavedDictionariesNamesAndConnnectionStrings;
        internal Dictionary<LanguageCode, List<Tuple<String, String>>> SavedVariantsNamesAndConnnectionStrings;

        [JsonConverter(typeof(StringEnumConverter))]
        internal LanguageCode NativeLanguage;
        [JsonConverter(typeof(StringEnumConverter))]
        internal LanguageCode TargetLanguage;

        internal Config Copy()
        {
            Config copy = new Config
            {
                FileStoreLocation = this.FileStoreLocation,
                DictionariesDirectory = this.DictionariesDirectory,
                VariantsDirectory = this.VariantsDirectory,

                NativeLanguage = this.NativeLanguage,
                TargetLanguage = this.TargetLanguage,

                SavedDictionariesNamesAndConnnectionStrings = new Dictionary<LanguageCode, List<Tuple<String, String>>>(),
                SavedVariantsNamesAndConnnectionStrings = new Dictionary<LanguageCode, List<Tuple<String, String>>>()
                
            };

            if (this.SavedDictionariesNamesAndConnnectionStrings is not null)
            {
                foreach (var entry in this.SavedDictionariesNamesAndConnnectionStrings)
                {
                    copy.SavedDictionariesNamesAndConnnectionStrings.Add(
                        entry.Key,
                        new List<Tuple<String, String>>(entry.Value));
                }
            }

            if (this.SavedVariantsNamesAndConnnectionStrings is not null)
            {
                foreach (var entry in this.SavedVariantsNamesAndConnnectionStrings)
                {
                    copy.SavedVariantsNamesAndConnnectionStrings.Add(
                        entry.Key,
                        new List<Tuple<String, String>>(entry.Value));
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

            bool sameFileStore = FileStoreLocation == rhs.FileStoreLocation;
            bool sameDictionaryDir = DictionariesDirectory == rhs.DictionariesDirectory;
            bool sameVariantsDir = VariantsDirectory == rhs.VariantsDirectory;

            // pigeon hole principle
            bool sameDictionaryDatabases = (SavedDictionariesNamesAndConnnectionStrings?.Count ?? 0) == (rhs.SavedDictionariesNamesAndConnnectionStrings?.Count ?? 0);
            if (sameDictionaryDatabases && (SavedDictionariesNamesAndConnnectionStrings?.Count ?? 0 ) > 0)
            {
                foreach (var entry in SavedDictionariesNamesAndConnnectionStrings)
                {
                    if (!rhs.SavedDictionariesNamesAndConnnectionStrings.TryGetValue(entry.Key, out var rhsValue))
                    {
                        sameDictionaryDatabases = false;
                        break;
                    }

                    sameDictionaryDatabases = sameDictionaryDatabases && entry.Value.SequenceEqual(rhsValue);
                    if (!sameDictionaryDatabases)
                    {
                        break;
                    }
                }
            }

            bool sameVariantsDatabases = (SavedVariantsNamesAndConnnectionStrings?.Count ?? 0) == (rhs.SavedVariantsNamesAndConnnectionStrings?.Count ?? 0);
            if (sameVariantsDatabases && (SavedVariantsNamesAndConnnectionStrings?.Count ?? 0) > 0)
            {
                foreach (var entry in SavedVariantsNamesAndConnnectionStrings)
                {
                    if (!rhs.SavedVariantsNamesAndConnnectionStrings.TryGetValue(entry.Key, out var rhsValue))
                    {
                        sameVariantsDatabases = false;
                        break;
                    }

                    sameVariantsDatabases = sameVariantsDatabases && entry.Value.SequenceEqual(rhsValue);
                    if (!sameVariantsDatabases)
                    {
                        break;
                    }
                }
            }

            return sameTarget && sameNative && sameFileStore && sameDictionaryDir && sameVariantsDir && sameDictionaryDatabases && sameVariantsDatabases;
        }

    }
}

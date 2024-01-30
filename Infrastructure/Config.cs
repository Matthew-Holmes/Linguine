using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure
{
    public class Config
    {
        public string FileStoreLocation;
        public string DictionariesDirectory;
        public Dictionary<LanguageCode, List<Tuple<String, String>>> SavedDictionariesNamesAndConnnectionStrings;

        public Config Copy()
        {
            Config copy = new Config
            {
                FileStoreLocation = this.FileStoreLocation,
                DictionariesDirectory = this.DictionariesDirectory,
                SavedDictionariesNamesAndConnnectionStrings = new Dictionary<LanguageCode, List<Tuple<String, String>>>()
            };

            foreach (var entry in this.SavedDictionariesNamesAndConnnectionStrings)
            {
                copy.SavedDictionariesNamesAndConnnectionStrings.Add(
                    entry.Key,
                    new List<Tuple<String, String>>(entry.Value));
            }

            return copy;
        }
    }
}

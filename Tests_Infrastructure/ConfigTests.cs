using Microsoft.VisualStudio.TestTools.UnitTesting;
using Infrastructure;
using System;
using System.Collections.Generic;


namespace Tests_Infrastructure
{
    [TestClass]
    public class ConfigTests
    {
        private Config originalConfig;
        private Config copiedConfig;

        [TestInitialize]
        public void Setup()
        {
            originalConfig = new Config
            {
                FileStoreLocation = "OriginalLocation",
                DictionariesDirectory = "OriginalDictionary",
                SavedDictionariesNamesAndConnnectionStrings = new Dictionary<LanguageCode, List<Tuple<String, String>>>
                {
                    { LanguageCode.eng, new List<Tuple<string, string>> { new Tuple<string, string>("Name1", "ConnectionString1") } }
                }
            };

            copiedConfig = originalConfig.Copy();
        }

        [TestMethod]
        public void Copy_FileStoreLocationIsIndependent()
        {
            copiedConfig.FileStoreLocation = "ChangedLocation";
            Assert.AreNotEqual(originalConfig.FileStoreLocation, copiedConfig.FileStoreLocation);
        }

        [TestMethod]
        public void Copy_DictionariesDirectoryIsIndependent()
        {
            copiedConfig.DictionariesDirectory = "ChangedDictionary";
            Assert.AreNotEqual(originalConfig.DictionariesDirectory, copiedConfig.DictionariesDirectory);
        }

        [TestMethod]
        public void Copy_SavedDictionariesNamesAndConnnectionStringsIsDeepCopied()
        {
            copiedConfig.SavedDictionariesNamesAndConnnectionStrings[LanguageCode.eng].Add(new Tuple<string, string>("Name2", "ConnectionString2"));
            Assert.AreNotEqual(originalConfig.SavedDictionariesNamesAndConnnectionStrings[LanguageCode.eng].Count, copiedConfig.SavedDictionariesNamesAndConnnectionStrings[LanguageCode.eng].Count);
        }

        [TestMethod]
        public void Equal_returnsTrueForCopy()
        {
            Assert.IsTrue(originalConfig.Equals(originalConfig.Copy()));
        }

        [TestMethod]
        public void Equal_returnsFalseForModifiedCopyAdditive()
        {
            Config modifiedCopy = originalConfig.Copy();
            modifiedCopy.SavedDictionariesNamesAndConnnectionStrings[LanguageCode.eng].Add(Tuple.Create("Dictionary2", "ConnectionString2"));
            Assert.IsFalse(originalConfig.Equals(modifiedCopy));
        }

        [TestMethod]
        public void Equal_returnsFalseForModifiedRemoved()
        {
            Config modifiedCopy = originalConfig.Copy();
            modifiedCopy.SavedDictionariesNamesAndConnnectionStrings[LanguageCode.eng] = new List<Tuple<string, string>>();
            Assert.IsFalse(originalConfig.Equals(modifiedCopy));
        }

        [TestMethod]
        public void Equal_returnsFalseForModifiedRemovedKeyEntirely()
        {
            Config modifiedCopy = originalConfig.Copy();
            modifiedCopy.SavedDictionariesNamesAndConnnectionStrings.Remove(LanguageCode.eng);
            Assert.IsFalse(originalConfig.Equals(modifiedCopy));
        }


    }
}

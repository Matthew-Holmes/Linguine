﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
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
                ConnectionStrings = new Dictionary<LanguageCode, String>
                {
                    { LanguageCode.eng, "ConnectionString1" }
                },
            };

            copiedConfig = originalConfig.Copy();
        }

        [TestMethod]
        public void Copy_OpenAIAPIIsIndependent()
        {
            copiedConfig.OpenAI_APIKeyLocation = "somethingelse.txt";
            Assert.AreNotEqual(originalConfig.OpenAI_APIKeyLocation, copiedConfig.OpenAI_APIKeyLocation);
        }

        [TestMethod]
        public void Copy_ConnectionStringsAreDeepCopied()
        {
            copiedConfig.ConnectionStrings[LanguageCode.eng] = "ConnectionString2";
            Assert.AreNotEqual(originalConfig.ConnectionStrings[LanguageCode.eng], copiedConfig.ConnectionStrings[LanguageCode.eng]);
        }

        [TestMethod]
        public void Equal_returnsTrueForCopy()
        {
            Assert.IsTrue(originalConfig.Equals(originalConfig.Copy()));
        }

        [TestMethod]
        public void Equal_returnsFalseForModifiedConnectionString()
        {
            Config modifiedCopy = originalConfig.Copy();
            modifiedCopy.ConnectionStrings[LanguageCode.eng] = "newConnString";
            Assert.IsFalse(originalConfig.Equals(modifiedCopy));
        }


        [TestMethod]
        public void Equal_returnsFalseRemovedKeyInConnectionStrings()
        {
            Config modifiedCopy = originalConfig.Copy();
            modifiedCopy.ConnectionStrings.Remove(LanguageCode.eng);
            Assert.IsFalse(originalConfig.Equals(modifiedCopy));
        }

        [TestMethod]
        public void Equal_returnsFalseAddKeyInConnectionStrings()
        {
            Config modifiedCopy = originalConfig.Copy();
            modifiedCopy.ConnectionStrings[LanguageCode.zho] = "ni hao";
            Assert.IsFalse(originalConfig.Equals(modifiedCopy));
        }
    }
}

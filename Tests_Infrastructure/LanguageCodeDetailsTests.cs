using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_Infrastructure
{
    [TestClass]
    public class LanguageCodeDetailsTests
    {
        [TestMethod]
        public void LanguagehName_AllEnumValueCombinations_ShouldNotReturnEmptyString()
        {
            var allLanguageCodes = Enum.GetValues(typeof(LanguageCode)).Cast<LanguageCode>().ToList();

            foreach (var nativeLanguageCode in allLanguageCodes)
            {
                foreach (var targetLanguageCode in allLanguageCodes)
                {
                    string languageName = LanguageCodeDetails.LanguageName(nativeLanguageCode, targetLanguageCode);
                    Assert.IsFalse(string.IsNullOrEmpty(languageName), $"Language name not implemented for native language '{nativeLanguageCode}' and target language '{targetLanguageCode}'");
                }
            }
        }
    }   
}

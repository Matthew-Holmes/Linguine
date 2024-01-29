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
        public void EnglishName_AllEnumValues_ShouldNotReturnEmptyString()
        {
            foreach (var code in Enum.GetValues(typeof(LanguageCode)).Cast<LanguageCode>())
            {
                var result = LanguageCodeDetails.EnglishName(code);
                Assert.AreNotEqual("", result, $"The method returned an empty string for LanguageCode.{code}");
            }
        }
    }
}

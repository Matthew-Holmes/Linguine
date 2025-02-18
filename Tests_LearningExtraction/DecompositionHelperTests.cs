using LearningExtraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infrastructure;

namespace Tests_LearningExtraction
{
    [TestClass]
    public class DecompositionHelperTests
    {

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetUnitLocations_NotInjective_Throws()
        {
            TextDecomposition td = TextDecomposition.FromNewLinedString("the total text", "the\nnoninjective\ndecomposition");

            DecompositionHelper.GetUnitLocations(td);

        }

        [TestMethod]
        public void GetUnitLocations_Injective_ResultRightLength()
        {
            TextDecomposition td = TextDecomposition.FromNewLinedString("the total text", "the\ntotal\ntext");

            List<int> ret = DecompositionHelper.GetUnitLocations(td);

            Assert.AreEqual(ret.Count, 3);
        }

        [TestMethod]
        public void GetUnitLocations_Injective_ResultCorrectLocations()
        {
            TextDecomposition td = TextDecomposition.FromNewLinedString("the total text", "the\ntotal\ntext");

            List<int> ret = DecompositionHelper.GetUnitLocations(td);

            Assert.AreEqual(ret[0], 0);
            Assert.AreEqual(ret[1], 4);
            Assert.AreEqual(ret[2], 10);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetContextWindows_NonInjective_Throws()
        {
            {
                TextDecomposition td = TextDecomposition.FromNewLinedString("the total text", "the\nnoninjective\ndecomposition");

                DecompositionHelper.GetContextWindows(td, 1, 1, 100);
            }
        }

        [TestMethod]
        public void GetContextWindows_Injective_ResultRightLength()
        {
            {
                TextDecomposition td = TextDecomposition.FromNewLinedString("a longer piece of text that will have context windows",
                    "a\nlonger\npiece\nof\ntext\nthat\nwill\nhave\ncontext\nwindows");

                var ret = DecompositionHelper.GetContextWindows(td, 1, 1, 100);

                Assert.AreEqual(ret.Count, 10);
            }
        }

        [TestMethod]
        public void GetContextWindows_Injective_IsNotNull()
        {
            {
                TextDecomposition td = TextDecomposition.FromNewLinedString("a longer piece of text that will have context windows",
                    "a\nlonger\npiece\nof\ntext\nthat\nwill\nhave\ncontext\nwindows");

                var ret = DecompositionHelper.GetContextWindows(td, 1, 1, 100);

                Assert.IsNotNull(ret);
            }
        }

        [TestMethod]
        public void GetContextWindows_Injective_CorrectWindows()
        {
            {
                TextDecomposition td = TextDecomposition.FromNewLinedString("a longer piece of text that will have context windows",
                    "a\nlonger\npiece\nof\ntext\nthat\nwill\nhave\ncontext\nwindows");

                var ret = DecompositionHelper.GetContextWindows(td, 1, 1, 100);

                Assert.AreEqual(ret[0], "a longer ");
                Assert.AreEqual(ret[1], "a longer piece ");
                Assert.AreEqual(ret[2], "longer piece of ");
                Assert.AreEqual(ret[3], "piece of text ");
                Assert.AreEqual(ret[4], "of text that ");
                Assert.AreEqual(ret[5], "text that will ");
                Assert.AreEqual(ret[6], "that will have ");
                Assert.AreEqual(ret[7], "will have context ");
                Assert.AreEqual(ret[8], "have context windows");
                Assert.AreEqual(ret[9], "context windows");
            }
        }

        [TestMethod]
        public void GetContextWindows_InjectiveContractsWindows_WindowsContainsUnit()
        {
            {
                TextDecomposition td = TextDecomposition.FromNewLinedString("a longer piece of text that will have context windows",
                    "a\nlonger\npiece\nof\ntext\nthat\nwill\nhave\ncontext\nwindows");

                var ret = DecompositionHelper.GetContextWindows(td, 2, 2, 20);

                Assert.IsTrue(ret[0].Contains("a"));
                Assert.IsTrue(ret[1].Contains("longer"));
                Assert.IsTrue(ret[2].Contains("piece"));
                Assert.IsTrue(ret[3].Contains("of"));
                Assert.IsTrue(ret[4].Contains("text"));
                Assert.IsTrue(ret[5].Contains("that"));
                Assert.IsTrue(ret[6].Contains("will"));
                Assert.IsTrue(ret[7].Contains("have"));
                Assert.IsTrue(ret[8].Contains("context"));
                Assert.IsTrue(ret[9].Contains("windows"));
            }
        }

        [TestMethod]
        public void GetContextWindows_InjectiveContractsWindows_WindowsBelowSizeLimit()
        {
            {
                TextDecomposition td = TextDecomposition.FromNewLinedString("a longer piece of text that will have context windows",
                    "a\nlonger\npiece\nof\ntext\nthat\nwill\nhave\ncontext\nwindows");

                var ret = DecompositionHelper.GetContextWindows(td, 2, 2, 20);

                for (int i = 0; i != 10; i++)
                {
                    Assert.IsTrue(ret[0].Length <= 20);
                }
            }
        }

        [TestMethod]
        public void GetContextWindows_InjectiveContractsWindows_WindowsNotTooSmall()
        {
            // only testing a necessary condition for the windows not being too small
            // not sufficient
            {
                TextDecomposition td = TextDecomposition.FromNewLinedString("a longer piece of text that will have context windows",
                    "a\nlonger\npiece\nof\ntext\nthat\nwill\nhave\ncontext\nwindows");

                var ret = DecompositionHelper.GetContextWindows(td, 2, 2, 20);

                for (int i = 0; i != 10; i++)
                {
                    Assert.IsTrue(ret[0].Length >= 12); // the longest words are 7 long, so if they are removed shouldn't remove any more
                }
            }
        }
    }
}

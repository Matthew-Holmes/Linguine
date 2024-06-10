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
        public void FromNewLinedString_CreatesCorrectDecomposition()
        {
            // Arrange
            string parent = "Parent text";
            string newLinedDecomposition = "Child text 1\nChild text 2";

            // Act
            TextDecomposition decomposition = DecompositionHelper.FromNewLinedString(parent, newLinedDecomposition);

            // Assert
            Assert.AreEqual(parent, decomposition.Total); // Assuming TextUnit has a Content property
            Assert.IsNotNull(decomposition.Decomposition);
            Assert.AreEqual(2, decomposition.Decomposition.Count);
            Assert.AreEqual("Child text 1", decomposition.Decomposition[0].Total);
            Assert.AreEqual("Child text 2", decomposition.Decomposition[1].Total);
        }

        [TestMethod]
        public void FromNewLinedString_EmptyStringCreatesLeaf()
        {
            string parent = "Parent text";
            string newLinedDecomposition = "";

            TextDecomposition decomposition = DecompositionHelper.FromNewLinedString(parent, newLinedDecomposition);

            Assert.AreEqual(parent, decomposition.Total);
            Assert.IsNull(decomposition.Decomposition);
        }

        [TestMethod]
        public void FromNewLinedString_NoNewLinesCreatesLeaf()
        {
            string parent = "Parent text";
            string newLinedDecomposition = "Parent text";

            TextDecomposition decomposition = DecompositionHelper.FromNewLinedString(parent, newLinedDecomposition);

            Assert.AreEqual(parent, decomposition.Total);
            Assert.IsNull(decomposition.Decomposition);
        }


        [TestMethod]
        public void Window_BelowThreshold_ReturnsSingleElementList()
        {
            var ret = DecompositionHelper.Window("a\nshort\npiece\nof\ntext", 500, 3);

            Assert.AreEqual(ret.Item1.Count, 1);
        }

        [TestMethod]
        public void Window_BelowThreshold_ReturnsSameStringInList()
        {
            var ret = DecompositionHelper.Window("a\nshort\npiece\nof\ntext", 500, 3);

            Assert.AreEqual(ret.Item1.First(), "a\nshort\npiece\nof\ntext");
        }

        [TestMethod]
        public void Window_BelowThreshold_DoesntReduceJoin()
        {
            var ret = DecompositionHelper.Window("a\nshort\npiece\nof\ntext", 500, 3);

            Assert.AreEqual(ret.Item2, 3);
        }

        [TestMethod]
        public void Window_BelowThresholdHiMaxChars_Runs()
        {
            var ret = DecompositionHelper.Window("a\nshort\npiece\nof\ntext", int.MaxValue, 3);
        }

        [TestMethod]
        public void Window_BelowThresholdEdgeZeroJoin_Runs()
        {
            var ret = DecompositionHelper.Window("a\nshort\npiece\nof\ntext", 500, 0);
        }

        [TestMethod]
        public void Window_AboveThresholdJoin1_CorrectNumberOfElementsInReturnList()
        {
            String toWindow = "1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd";

            var ret = DecompositionHelper.Window(toWindow, 15, 1);

            Assert.AreEqual(ret.Item1.Count, 6);
        }

        [TestMethod]
        public void Window_AboveThresholdJoin1_DoesntReduceJoin()
        {
            String toWindow = "1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd";

            var ret = DecompositionHelper.Window(toWindow, 15, 1);

            Assert.AreEqual(ret.Item2, 1);
        }

        [TestMethod]
        public void Window_AboveThresholdJoin1_CorrectWindows()
        {
            String toWindow = "1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd";

            var ret = DecompositionHelper.Window(toWindow, 15, 1);

            Assert.AreEqual(ret.Item1[0], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[1], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[2], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[3], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[4], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[5], "1234\nabcd");
        }

        [TestMethod]
        public void Window_AboveThresholdJoin2_CorrectNumberOfElementsInReturnList()
        {
            String toWindow = "1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd";

            var ret = DecompositionHelper.Window(toWindow, 15, 2);

            Assert.AreEqual(ret.Item1.Count, 10);
        }

        [TestMethod]
        public void Window_AboveThresholdJoin2_DoesntReduceJoin()
        {
            String toWindow = "1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd";

            var ret = DecompositionHelper.Window(toWindow, 15, 2);

            Assert.AreEqual(ret.Item2, 2);
        }

        [TestMethod]
        public void Window_AboveThresholdJoin2_CorrectWindows()
        {
            String toWindow = "1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd";

            var ret = DecompositionHelper.Window(toWindow, 15, 2);

            Assert.AreEqual(ret.Item1[0], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[1], "abcd\n1234\nabcd");
            Assert.AreEqual(ret.Item1[2], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[3], "abcd\n1234\nabcd");
            Assert.AreEqual(ret.Item1[4], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[5], "abcd\n1234\nabcd");
            Assert.AreEqual(ret.Item1[6], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[7], "abcd\n1234\nabcd");
            Assert.AreEqual(ret.Item1[8], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[9], "abcd\n1234\nabcd");
        }

        [TestMethod]
        public void Window_LowCharHighJoin_ReducesJoin()
        {
            String toWindow = "1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd";

            var ret = DecompositionHelper.Window(toWindow, 15, 4);

            Assert.AreEqual(ret.Item2, 2);
        }

        [TestMethod]
        public void Window_LowCharHighJoin_CorrectWindows()
        {
            String toWindow = "1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd";

            var ret = DecompositionHelper.Window(toWindow, 15, 4);

            Assert.AreEqual(ret.Item1[0], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[1], "abcd\n1234\nabcd");
            Assert.AreEqual(ret.Item1[2], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[3], "abcd\n1234\nabcd");
            Assert.AreEqual(ret.Item1[4], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[5], "abcd\n1234\nabcd");
            Assert.AreEqual(ret.Item1[6], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[7], "abcd\n1234\nabcd");
            Assert.AreEqual(ret.Item1[8], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[9], "abcd\n1234\nabcd");

            Assert.AreEqual(ret.Item1.Count, 10);
        }

        [TestMethod]
        public void Window_AboveThresholdJoin1Indivisible_CorrectWindows()
        {
            String toWindow = "1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd";

            var ret = DecompositionHelper.Window(toWindow, 17, 1);

            Assert.AreEqual(ret.Item1[0], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[1], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[2], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[3], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[4], "1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[5], "1234\nabcd");
        }

        [TestMethod]
        public void Window_JoinAndCore_DoesntReduceJoin()
        {
            String toWindow = "1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd";

            var ret = DecompositionHelper.Window(toWindow, 25, 2);

            Assert.AreEqual(ret.Item2, 2);
        }

        [TestMethod]
        public void Window_JoinAndCore_CorrectNumberOfWindows()
        {
            String toWindow = "1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd";

            var ret = DecompositionHelper.Window(toWindow, 25, 2);

            Assert.AreEqual(ret.Item1.Count, 4);
        }

        [TestMethod]
        public void Window_JoinAndCore_CorrectWindows()
        {
            String toWindow = "1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd\n1234\nabcd";

            var ret = DecompositionHelper.Window(toWindow, 25, 2);

            Assert.AreEqual(ret.Item1[0], "1234\nabcd\n1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[1], "abcd\n1234\nabcd\n1234\nabcd");
            Assert.AreEqual(ret.Item1[2], "1234\nabcd\n1234\nabcd\n1234");
            Assert.AreEqual(ret.Item1[3], "abcd\n1234\nabcd");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void GetUnitLocations_NotInjective_Throws()
        {
            TextDecomposition td = DecompositionHelper.FromNewLinedString("the total text", "the\nnoninjective\ndecomposition");

            DecompositionHelper.GetUnitLocations(td);

        }

        [TestMethod]
        public void GetUnitLocations_Injective_ResultRightLength()
        {
            TextDecomposition td = DecompositionHelper.FromNewLinedString("the total text", "the\ntotal\ntext");

            List<int> ret = DecompositionHelper.GetUnitLocations(td);

            Assert.AreEqual(ret.Count, 3);
        }

        [TestMethod]
        public void GetUnitLocations_Injective_ResultCorrectLocations()
        {
            TextDecomposition td = DecompositionHelper.FromNewLinedString("the total text", "the\ntotal\ntext");

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
                TextDecomposition td = DecompositionHelper.FromNewLinedString("the total text", "the\nnoninjective\ndecomposition");

                DecompositionHelper.GetContextWindows(td, 1, 1, 100);
            }
        }

        [TestMethod]
        public void GetContextWindows_Injective_ResultRightLength()
        {
            {
                TextDecomposition td = DecompositionHelper.FromNewLinedString("a longer piece of text that will have context windows",
                    "a\nlonger\npiece\nof\ntext\nthat\nwill\nhave\ncontext\nwindows");

                var ret = DecompositionHelper.GetContextWindows(td, 1, 1, 100);

                Assert.AreEqual(ret.Count, 10);
            }
        }

        [TestMethod]
        public void GetContextWindows_Injective_IsNotNull()
        {
            {
                TextDecomposition td = DecompositionHelper.FromNewLinedString("a longer piece of text that will have context windows",
                    "a\nlonger\npiece\nof\ntext\nthat\nwill\nhave\ncontext\nwindows");

                var ret = DecompositionHelper.GetContextWindows(td, 1, 1, 100);

                Assert.IsNotNull(ret);
            }
        }

        [TestMethod]
        public void GetContextWindows_Injective_CorrectWindows()
        {
            {
                TextDecomposition td = DecompositionHelper.FromNewLinedString("a longer piece of text that will have context windows",
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
                TextDecomposition td = DecompositionHelper.FromNewLinedString("a longer piece of text that will have context windows",
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
                TextDecomposition td = DecompositionHelper.FromNewLinedString("a longer piece of text that will have context windows",
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
                TextDecomposition td = DecompositionHelper.FromNewLinedString("a longer piece of text that will have context windows",
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

using ExternalMedia;
using Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_ExternalMedia
{
    [TestClass]
    public class TextualMediaHelperTests
    {
        [TestMethod]
        public void Windowed_BelowWindowingThreshold_ReturnsSingleElementList()
        {
            TextualMedia concise = new TextualMedia { Text = "short" };

            List<String> windowed = TextualMediaHelper.Windowed(concise, 100, 10, 10);

            Assert.IsNotNull(windowed);
            Assert.AreEqual(1, windowed.Count);
            Assert.AreEqual(windowed.First(), "short");
        }

        [TestMethod]
        public void Windowed_NoJoinNoPadding_ReturnsWindows()
        {
            TextualMedia testText = new TextualMedia { Text = "1234567890abcdefghij" };

            List<String> windowed = TextualMediaHelper.Windowed(testText, 5, 0, 0);

            Assert.IsNotNull(windowed);
            Assert.AreEqual(4, windowed.Count);
            Assert.AreEqual(windowed[0], "12345");
            Assert.AreEqual(windowed[1], "67890");
            Assert.AreEqual(windowed[2], "abcde");
            Assert.AreEqual(windowed[3], "fghij");
        }

        public void Windowed_NoJoinNoPaddingNotDivisible_ReturnsWindows()
        {
            TextualMedia testText = new TextualMedia { Text = "1234567890abcdefghijend" };

            List<String> windowed = TextualMediaHelper.Windowed(testText, 5, 0, 0);

            Assert.IsNotNull(windowed);
            Assert.AreEqual(5, windowed.Count);
            Assert.AreEqual(windowed[0], "12345");
            Assert.AreEqual(windowed[1], "67890");
            Assert.AreEqual(windowed[2], "abcde");
            Assert.AreEqual(windowed[3], "fghij");
            Assert.AreEqual(windowed[4], "end");
        }

        [TestMethod]
        public void Windowed_JoinNoPadding_ReturnsWindows()
        {
            TextualMedia testText = new TextualMedia { Text = "1234567890abcdefghij" };

            List<String> windowed = TextualMediaHelper.Windowed(testText, 8, 2, 0);

            Assert.IsNotNull(windowed);
            Assert.AreEqual(4, windowed.Count);       /* left core right*/
            Assert.AreEqual(windowed[0], "123456");   /*      1234 56*/
            Assert.AreEqual(windowed[1], "567890ab"); /* 56   7890 ab */
            Assert.AreEqual(windowed[2], "abcdefgh"); /* ab   cdef gh */
            Assert.AreEqual(windowed[3], "ghij");     /* gh   ij      */
        }


        [TestMethod]
        public void Windowed_PaddingNoJoin_ReturnsWindows()
        {
            TextualMedia testText = new TextualMedia { Text = "1234567890abcdefghij" };

            List<String> windowed = TextualMediaHelper.Windowed(testText, 8, 0, 2);

            Assert.IsNotNull(windowed);
            Assert.AreEqual(5, windowed.Count);       /* left core right*/
            Assert.AreEqual(windowed[0], "123456");   /*      1234 56*/
            Assert.AreEqual(windowed[1], "34567890"); /* 34   5678 90 */
            Assert.AreEqual(windowed[2], "7890abcd"); /* 78   90ab cd */
            Assert.AreEqual(windowed[3], "abcdefgh"); /* ab   cdef gh */
            Assert.AreEqual(windowed[4], "efghij");   /* ef   ghij    */
        }

        [TestMethod]
        public void Windowed_PaddingNoJoinNotDivisible_ReturnsWindows()
        {
            TextualMedia testText = new TextualMedia { Text = "1234567890abcdefghijk" };

            List<String> windowed = TextualMediaHelper.Windowed(testText, 8, 0, 2);

            Assert.IsNotNull(windowed);
            Assert.AreEqual(6, windowed.Count);       /* left core right*/
            Assert.AreEqual(windowed[0], "123456");   /*      1234 56 */
            Assert.AreEqual(windowed[1], "34567890"); /* 34   5678 90 */
            Assert.AreEqual(windowed[2], "7890abcd"); /* 78   90ab cd */
            Assert.AreEqual(windowed[3], "abcdefgh"); /* ab   cdef gh */
            Assert.AreEqual(windowed[4], "efghijk");  /* ef   ghij k  */
            Assert.AreEqual(windowed[5], "ijk");      /* ij   k       */
        }

        [TestMethod]
        public void Windowed_JoinAndPadding_ReturnsWindows()
        {
            TextualMedia testText = new TextualMedia { Text = "1234567890abcdefghijkl" };

            List<String> windowed = TextualMediaHelper.Windowed(testText, 13, 2, 3);

            Assert.IsNotNull(windowed);
            Assert.AreEqual(5, windowed.Count);            /* lp  lj  core rj  rp  */
            Assert.AreEqual(windowed[0], "12345678");      /*         123  45  678 */
            Assert.AreEqual(windowed[1], "1234567890abc"); /* 123 45  678  90  abc */
            Assert.AreEqual(windowed[2], "67890abcdefgh"); /* 678 90  abc  de  fgh */
            Assert.AreEqual(windowed[3], "abcdefghijkl");  /* abc de  fgh  ij  kl  */
            Assert.AreEqual(windowed[4], "fghijkl");       /* fgh ij  kl           */
        }

        [TestMethod]
        public void Windowed_JoinAndPadding_ReturnsWindowsFinalCoreEmpty()
        {
            // don't produce a final term since joins are expected to be entirely processed
            // this behaviour is the most nuanced

            TextualMedia testText = new TextualMedia { Text = "1234567890abcdefghij" };

            List<String> windowed = TextualMediaHelper.Windowed(testText, 13, 2, 3);

            Assert.IsNotNull(windowed);
            Assert.AreEqual(4, windowed.Count);            /* lp  lj  core rj  rp  */
            Assert.AreEqual(windowed[0], "12345678");      /*         123  45  678 */
            Assert.AreEqual(windowed[1], "1234567890abc"); /* 123 45  678  90  abc */
            Assert.AreEqual(windowed[2], "67890abcdefgh"); /* 678 90  abc  de  fgh */
            Assert.AreEqual(windowed[3], "abcdefghij");    /* abc de  fgh  ij      */
        }

        #region Extreme but valid values
        [TestMethod]
        public void Windowed_WidePad_NoThrow()
        {
            TextualMedia testText = new TextualMedia { Text = new String('c', 100) };

            List<String> windowed1 = TextualMediaHelper.Windowed(testText, 100, 0, 0);
            List<String> windowed2 = TextualMediaHelper.Windowed(testText, 110, 0, 50);
        }

        [TestMethod]
        public void Windowed_RelativelyWidePad_NoThrow()
        {
            TextualMedia testText = new TextualMedia { Text = new String('c', 100) };

            List<String> windowed1 = TextualMediaHelper.Windowed(testText, 100, 0, 0);
            List<String> windowed2 = TextualMediaHelper.Windowed(testText, 50, 0, 20);
        }

        [TestMethod]
        public void Windowed_VeryWidePad_NoThrow()
        {
            TextualMedia testText = new TextualMedia { Text = new String('c', 100) };

            List<String> windowed2 = TextualMediaHelper.Windowed(testText, 410, 0, 200);
        }

            [TestMethod]
        public void Windowed_VeryWideJoin_NoThrow()
        {
            TextualMedia testText = new TextualMedia { Text = new String('c', 100) };

            List<String> windowed2 = TextualMediaHelper.Windowed(testText, 410, 200, 0);
        }

        public void Windowed_WideJoin_NoThrow()
        {
            TextualMedia testText = new TextualMedia { Text = new String('c', 100) };

            List<String> windowed2 = TextualMediaHelper.Windowed(testText, 110, 50, 0);
        }

        public void Windowed_RelativelyWideJoin_NoThrow()
        {
            TextualMedia testText = new TextualMedia { Text = new String('c', 100) };

            List<String> windowed2 = TextualMediaHelper.Windowed(testText, 50, 20, 0);
        }

        [TestMethod]
        public void Windowed_WideCore_NoThrow()
        {
            TextualMedia testText = new TextualMedia { Text = new String('c', 100) };

            List<String> windowed2 = TextualMediaHelper.Windowed(testText, 1000, 0, 0);
        }

        [TestMethod]
        public void Windowed_WidePadAndJoin_NoThrow()
        {
            TextualMedia testText = new TextualMedia { Text = new String('c', 100) };

            List<String> windowed2 = TextualMediaHelper.Windowed(testText, 210, 50, 50);
        }

        [TestMethod]
        public void Windowed_RelativelyWidePadAndJoin_NoThrow()
        {
            TextualMedia testText = new TextualMedia { Text = new String('c', 100) };

            List<String> windowed2 = TextualMediaHelper.Windowed(testText, 90, 20, 20);
        }

        [TestMethod]
        public void Windowed_VeryWidePadAndJoin_NoThrow()
        {
            TextualMedia testText = new TextualMedia { Text = new String('c', 100) };

            List<String> windowed2 = TextualMediaHelper.Windowed(testText, 810, 200, 200);
        }

        [TestMethod]
        public void Windowed_QuiteWidePadAndJoin_NoThrow()
        {
            TextualMedia testText = new TextualMedia { Text = new String('c', 100) };

            List<String> windowed2 = TextualMediaHelper.Windowed(testText, 50, 10, 10);
        }

        [TestMethod]
        public void Windowed_EmptyCoreButHasJoins_NoThrow()
        {
            TextualMedia testText = new TextualMedia { Text = new String('c', 100) };

            List<String> windowed2 = TextualMediaHelper.Windowed(testText, 40, 10, 10);
        }

        #endregion

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Windowed_NegativeValueForWindowChars_Throws()
        {

            TextualMedia testText = new TextualMedia { Text = new String('c', 100) };

            List<String> windowed = TextualMediaHelper.Windowed(testText, -1, 10, 10);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Windowed_NegativeValueForJoinChars_Throws()
        {
            TextualMedia testText = new TextualMedia { Text = new String('c', 100) };

            List<String> windowed = TextualMediaHelper.Windowed(testText, 50, -1, 10);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Windowed_NegativeValueForPadChars_Throws()
        {
            TextualMedia testText = new TextualMedia { Text = new String('c', 100) };

            List<String> windowed = TextualMediaHelper.Windowed(testText, 50, 10, -1);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Windowed_PaddingButNothingInside_Throws()
        {
            TextualMedia testText = new TextualMedia { Text = new String('c', 100) };

            List<String> windowed = TextualMediaHelper.Windowed(testText, 10, 0, 10);
        }

    }
}

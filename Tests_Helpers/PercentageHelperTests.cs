using Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_Helpers
{
    [TestClass]
    public class PercentageHelperTests
    {
        [TestMethod]
        public void TestFarApartPercentages()
        {
            List<decimal> percentages = new List<decimal> { 1.456m, 34.563m, 99.999m };
            List<decimal> target = new List<decimal> { 1m, 35m, 100m };
            List<decimal> rounded = PercentageHelper.RoundDistinctPercentages(percentages);

            // Check if rounded list contains all distinct percentages
            CollectionAssert.AreEqual(rounded, target);
        }

        [TestMethod]
        public void TestSomeClosePercentages()
        {
            List<decimal> percentages = new List<decimal> { 1.456m, 34.563m, 34.789m, 99.999m };
            List<decimal> target = new List<decimal> { 1m, 34.6m, 34.8m, 100m };
            List<decimal> rounded = PercentageHelper.RoundDistinctPercentages(percentages);

            // Check if rounded list contains all distinct percentages
            CollectionAssert.AreEqual(rounded, target);
        }

        [TestMethod]
        public void TestSomeClosePercentagesDifferentProximities()
        {
            List<decimal> percentages = new List<decimal> { 1.456m, 34.563m, 34.789m, 99.999m, 40.123m, 40.1236m };
            List<decimal> target = new List<decimal> { 1m, 34.6m, 34.8m, 100m, 40.123m, 40.124m };
            List<decimal> rounded = PercentageHelper.RoundDistinctPercentages(percentages);

            // Check if rounded list contains all distinct percentages
            CollectionAssert.AreEqual(rounded, target);
        }

        [TestMethod]
        public void TestVeryClosePercentages()
        {
            List<decimal> percentages = new List<decimal> { 1.456m, 34.563m, 34.789m, 99.999m, 40.0000123m, 40.00001236m };
            List<decimal> target = new List<decimal> { 1m, 34.6m, 34.8m, 100m, 40.0000123m, 40.0000124m };
            List<decimal> rounded = PercentageHelper.RoundDistinctPercentages(percentages);

            // Check if rounded list contains all distinct percentages
            CollectionAssert.AreEqual(rounded, target);
        }

        [TestMethod]
        public void TestSomeClosePercentagesButFine()
        {
            List<decimal> percentages = new List<decimal> { 1.456m, 34.563m, 34.289m, 99.999m };
            List<decimal> target = new List<decimal> { 1m, 35m, 34m, 100m };
            List<decimal> rounded = PercentageHelper.RoundDistinctPercentages(percentages);

            // Check if rounded list contains all distinct percentages
            CollectionAssert.AreEqual(rounded, target);
        }

        [TestMethod]
        public void TestReturnsDistinctPercentages()
        {
            List<decimal> percentages = new List<decimal> { 45.5m, 45.49m, 46.51m, 46.52m };
            List<decimal> rounded = PercentageHelper.RoundDistinctPercentages(percentages);

            // Check if rounded list contains all distinct percentages
            Assert.AreEqual(rounded.Distinct().Count(), rounded.Count);
        }

        [TestMethod]
        public void TestPrecisionIncrease()
        {
            List<decimal> percentages = new List<decimal> { 45.5m, 45.49m, 45.51m, 46.49m, 46.51m, 46.52m };
            List<decimal> rounded = PercentageHelper.RoundDistinctPercentages(percentages);

            // Check if rounded list contains all distinct percentages
            Assert.AreEqual(rounded.Distinct().Count(), rounded.Count);
        }

        [TestMethod]
        public void TestLargeDataSet()
        {
            List<decimal> percentages = Enumerable.Range(0, 1000).Select(x => (decimal)x / 10).ToList();
            List<decimal> rounded = PercentageHelper.RoundDistinctPercentages(percentages);

            // Check if rounded list contains all distinct percentages
            Assert.AreEqual(rounded.Distinct().Count(), rounded.Count);
        }
    }
}

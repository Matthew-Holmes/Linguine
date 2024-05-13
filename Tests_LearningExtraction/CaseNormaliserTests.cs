using Agents;
using ExternalMedia;
using Infrastructure;
using LearningExtraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_LearningExtraction
{
    [TestClass]
    public class CaseNormaliserTests
    {
        // TODO - add checking for if the maxvolume/join numbers are invalid
        // TODO - add tests for nest TextDecompositions

        private class EverythingLowercaseAgent : AgentBase
        {
            protected override Task<String> GetResponseCore(string prompt)
            {
                return Task.FromResult(prompt.ToLower());
            }
        }

        [TestMethod]
        public void NormaliseCases_NoChunking_Runs()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 1000;

            TextDecomposition source = TextDecomposition.FromNewLinedString("Hello Bob", "Hello\nBob");

            var result = normaliser.NormaliseCases(source).Result;
        }

        [TestMethod]
        public void NormaliseCases_NoChunking_PreservesParentTotal()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 1000;

            TextDecomposition source = TextDecomposition.FromNewLinedString("Hello Bob", "Hello\nBob");

            var result = normaliser.NormaliseCases(source).Result;

            Assert.AreEqual(source.Total.Text, "Hello Bob");
        }

        [TestMethod]
        public void NormaliseCases_NoChunking_ResultPreservesTotal()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 1000;

            TextDecomposition source = TextDecomposition.FromNewLinedString("Hello Bob", "Hello\nBob");

            var result = normaliser.NormaliseCases(source).Result;

            Assert.AreEqual(result.Total.Text, "Hello Bob");
        }

        [TestMethod]
        public void NormaliseCases_NoChunking_PreservesParentUnitsTotals()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 1000;

            TextDecomposition source = TextDecomposition.FromNewLinedString("Hello Bob", "Hello\nBob");

            var result = normaliser.NormaliseCases(source).Result;

            Assert.AreEqual(source.Decomposition[0].Total.Text, "Hello");
            Assert.AreEqual(source.Decomposition[1].Total.Text, "Bob");
        }

        [TestMethod]
        public void NormaliseCases_NoChunking_PreservesParentUnitsNumber()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 1000;

            TextDecomposition source = TextDecomposition.FromNewLinedString("Hello Bob", "Hello\nBob");

            var result = normaliser.NormaliseCases(source).Result;

            Assert.AreEqual(source.Decomposition.Count, 2);
        }

        [TestMethod]
        public void NormaliseCases_NoChunking_PreservesParentUnitsLeafStatus()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 1000;

            TextDecomposition source = TextDecomposition.FromNewLinedString("Hello Bob", "Hello\nBob");

            var result = normaliser.NormaliseCases(source).Result;

            Assert.IsNull(source.Decomposition[0].Decomposition);
            Assert.IsNull(source.Decomposition[1].Decomposition);
        }

        [TestMethod]
        public void NormaliseCases_NoChunking_PreservesDecompositionSize()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 1000;

            TextDecomposition source = TextDecomposition.FromNewLinedString("Hello Bob", "Hello\nBob");

            var result = normaliser.NormaliseCases(source).Result;

            Assert.AreEqual(result.Decomposition.Count, 2);
        }

        [TestMethod]
        public void NormaliseCases_NoChunking_PreservesLeaves()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 1000;

            TextDecomposition source = TextDecomposition.FromNewLinedString("Hello Bob", "Hello\nBob");

            var result = normaliser.NormaliseCases(source).Result;

            Assert.IsNull(result.Decomposition[0].Decomposition);
            Assert.IsNull(result.Decomposition[1].Decomposition);
        }

        [TestMethod]
        public void NormaliseCases_NoChunking_AppliesAgent()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 1000;

            TextDecomposition source = TextDecomposition.FromNewLinedString("Hello Bob", "Hello\nBob");

            var result = normaliser.NormaliseCases(source).Result;

            Assert.AreEqual(result.Decomposition[0].Total.Text, "hello");
            Assert.AreEqual(result.Decomposition[1].Total.Text, "bob");
        }

        [TestMethod]
        public void NormaliseCases_Chunking_Runs()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 50;
            normaliser.JoinLineCount = 3;

            String longText = string.Join(" ", Enumerable.Repeat("Word", 100));
            String longUnits = string.Join("\n", Enumerable.Repeat("Word", 100));

            TextDecomposition source = TextDecomposition.FromNewLinedString(longText, longUnits);

            var result = normaliser.NormaliseCases(source).Result;
        }

        [TestMethod]
        public void NormaliseCases_Chunking_PreservesNumberOfUnits()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 50;
            normaliser.JoinLineCount = 3;

            String longText = string.Join(" ", Enumerable.Repeat("Word", 100));
            String longUnits = string.Join("\n", Enumerable.Repeat("Word", 100));

            TextDecomposition source = TextDecomposition.FromNewLinedString(longText, longUnits);

            var result = normaliser.NormaliseCases(source).Result;

            Assert.AreEqual(result.Decomposition.Count, 100);
        }

        [TestMethod]
        public void NormaliseCases_Chunking_PreservesParentNumberOfUnits()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 50;
            normaliser.JoinLineCount = 3;

            String longText = string.Join(" ", Enumerable.Repeat("Word", 100));
            String longUnits = string.Join("\n", Enumerable.Repeat("Word", 100));

            TextDecomposition source = TextDecomposition.FromNewLinedString(longText, longUnits);

            var result = normaliser.NormaliseCases(source).Result;

            Assert.AreEqual(source.Decomposition.Count, 100);
        }

        [TestMethod]
        public void NormaliseCases_Chunking_AppliesAgent()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 50;
            normaliser.JoinLineCount = 3;

            String longText = string.Join(" ", Enumerable.Repeat("Word", 100));
            String longUnits = string.Join("\n", Enumerable.Repeat("Word", 100));

            TextDecomposition source = TextDecomposition.FromNewLinedString(longText, longUnits);

            var result = normaliser.NormaliseCases(source).Result;

            for (int i = 0; i != 100; i++)
            {
                Assert.AreEqual(result.Decomposition[i].Total.Text, "word");
            }
        }

        [TestMethod]
        public void NormaliseCases_Chunking_PreservesParentUnits()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 50;
            normaliser.JoinLineCount = 3;

            String longText = string.Join(" ", Enumerable.Repeat("Word", 100));
            String longUnits = string.Join("\n", Enumerable.Repeat("Word", 100));

            TextDecomposition source = TextDecomposition.FromNewLinedString(longText, longUnits);

            var result = normaliser.NormaliseCases(source).Result;

            for (int i = 0; i != 100; i++)
            {
                Assert.AreEqual(source.Decomposition[i].Total.Text, "Word");
            }
        }

        [TestMethod]
        public void NormaliseCases_Chunking_PreservesLeafStatus()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 50;
            normaliser.JoinLineCount = 3;

            String longText = string.Join(" ", Enumerable.Repeat("Word", 100));
            String longUnits = string.Join("\n", Enumerable.Repeat("Word", 100));

            TextDecomposition source = TextDecomposition.FromNewLinedString(longText, longUnits);

            var result = normaliser.NormaliseCases(source).Result;

            for (int i = 0; i != 100; i++)
            {
                Assert.IsNull(result.Decomposition[i].Decomposition);
            }
        }

        [TestMethod]
        public void NormaliseCases_Chunking_PreservesParentLeafStatus()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 50;
            normaliser.JoinLineCount = 3;

            String longText = string.Join(" ", Enumerable.Repeat("Word", 100));
            String longUnits = string.Join("\n", Enumerable.Repeat("Word", 100));

            TextDecomposition source = TextDecomposition.FromNewLinedString(longText, longUnits);

            var result = normaliser.NormaliseCases(source).Result;

            for (int i = 0; i != 100; i++)
            {
                Assert.IsNull(source.Decomposition[i].Decomposition);
            }
        }

        [TestMethod]
        public void NormaliseCases_Chunking_PreservesParentTotal()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 50;
            normaliser.JoinLineCount = 3;

            String longText = string.Join(" ", Enumerable.Repeat("Word", 100));
            String longUnits = string.Join("\n", Enumerable.Repeat("Word", 100));

            TextDecomposition source = TextDecomposition.FromNewLinedString(longText, longUnits);

            var result = normaliser.NormaliseCases(source).Result;

            Assert.AreEqual(source.Total.Text, longText);
        }

        [TestMethod]
        public void NormaliseCases_Chunking_PreservesTotal()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 50;
            normaliser.JoinLineCount = 3;

            String longText = string.Join(" ", Enumerable.Repeat("Word", 100));
            String longUnits = string.Join("\n", Enumerable.Repeat("Word", 100));

            TextDecomposition source = TextDecomposition.FromNewLinedString(longText, longUnits);

            var result = normaliser.NormaliseCases(source).Result;

            Assert.AreEqual(result.Total.Text, longText);
        }

        [TestMethod]
        public void NormaliseCases_ChunkingBigJoin_Runs()
        {
            CaseNormaliser normaliser = new CaseNormaliser();
            normaliser.Agent = new EverythingLowercaseAgent();
            normaliser.MaxVolumeToProcess = 100;
            normaliser.JoinLineCount = 30;

            String longText = string.Join(" ", Enumerable.Repeat("Word", 100));
            String longUnits = string.Join("\n", Enumerable.Repeat("Word", 100));

            TextDecomposition source = TextDecomposition.FromNewLinedString(longText, longUnits);

            var result = normaliser.NormaliseCases(source).Result;
        }

    }
}

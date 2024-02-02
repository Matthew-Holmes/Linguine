using Microsoft.VisualStudio.TestTools.UnitTesting;
using LearningExtraction;
using Agents.DummyAgents;
using System;
using ExternalMedia;
using Infrastructure;
using Agents;

namespace Tests_LearningExtraction
{
    [TestClass]
    public class TextDecomposerTests
    {

        private class WontBijectWillInjectAgent : AgentBase
        {
            protected override string GetResponseCore(string prompt)
            {
                return prompt.Substring(0, 5);
            }
        }


        private class WontlInjectAgent : AgentBase
        {
            protected override string GetResponseCore(string prompt)
            {
                return "Here's your decomposition:\n" + prompt.Substring(0, 5);
            }
        }


        [TestMethod]
        public void DecomposeText_WithShortText_ShouldNotChunk()
        {
            // Arrange
            var agent = new DummyTextDecompositionAgent();
            var decomposer = new TextDecomposer(10, agent); // Set a max volume to process that is larger than the text
            var textSource = new TextualMedia("Hello", LanguageCode.eng);

            // Act
            var result = decomposer.DecomposeText(textSource);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.Units); // should be a leaf
            Assert.IsTrue(result.Total.Text == "Hello");
        }

        [TestMethod]
        public void DecomposeText_WithLongText_ShouldDecomposeCorrectly()
        {
            // Arrange
            var agent = new DummyTextDecompositionAgent();
            var decomposer = new TextDecomposer(50, agent); // Set a max volume to process that can handle the text
            var textSource = new TextualMedia("This is a longer text for testing purposes", LanguageCode.fra);

            // Act
            var result = decomposer.DecomposeText(textSource);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(textSource.Text, result.Total.Text);
            Assert.IsTrue(result.Units != null && result.Units.Count > 0); // Ensure decomposition happened
            Assert.IsTrue(result.Injects()); // Check if the decomposition correctly injects
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Invalid decomposition")]
        public void DecomposeText_WithBijectRequirementNotMet_ShouldThrowException()
        {
            // Arrange
            var decomposer = new TextDecomposer(100, new WontBijectWillInjectAgent());
            var textSource = new TextualMedia("This is a longer text for testing biject requirements", LanguageCode.eng);

            // Act
            decomposer.DecomposeText(textSource, mustBiject: true);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception), "Invalid decomposition")]
        public void DecomposeText_WithInjectRequirementNotMet_ShouldThrowException()
        {
            // Arrange
            var agent = new DummyTextDecompositionAgent();
            var decomposer = new TextDecomposer(50, new WontlInjectAgent());
            var textSource = new TextualMedia("This text is not expected to inject properly", LanguageCode.eng);

            // Act
            decomposer.DecomposeText(textSource, mustInject: true);
        }
    }
}

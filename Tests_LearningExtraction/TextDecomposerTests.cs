﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using LearningExtraction;
using Agents.DummyAgents;
using System;
using ExternalMedia;
using Infrastructure;
using Agents;
using System.Security.Cryptography;
using System.Xml;

namespace Tests_LearningExtraction
{
    [TestClass]
    public class TextDecomposerTests
    {

        private class WontBijectWillInjectAgent : AgentBase
        {
            protected override Task<String> GetResponseCore(string prompt)
            {
                return Task.FromResult(prompt.Substring(0, 5));
            }
        }


        private class WontlInjectAgent : AgentBase
        {
            protected override Task<String> GetResponseCore(string prompt)
            {
                return Task.FromResult("Here's your decomposition:\n" + prompt.Substring(0, 5));
            }
        }


        [TestMethod]
        public void DecomposeText_WithShortText_ShouldNotChunk()
        {
            // Arrange
            var agent = new DummyTextDecompositionAgent();
            var decomposer = new TextDecomposer(); // Set a max volume to process that is larger than the text
            decomposer.StandardAgent = agent;
            decomposer.HighPerformanceAgent = agent;
            decomposer.FallbackAgent = agent;
            decomposer.MaxVolumeToProcess = 10;
            var textSource = new TextualMedia { Text = "Hello" };

            // Act
            var result = decomposer.DecomposeText(textSource.Text).Result;

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.Decomposition); // should be a leaf
            Assert.IsTrue(result.Total == "Hello");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void DecomposeText_WithLongText_ShouldThrow()
        {
            // Arrange
            var agent = new WhitespaceDecompositionAgent();
            var decomposer = new TextDecomposer(/*50, agent*/); // Set a max volume to process that can handle the text
            decomposer.StandardAgent = agent;
            decomposer.HighPerformanceAgent = agent;
            decomposer.FallbackAgent = agent;
            decomposer.MaxVolumeToProcess = 50;
            var textSource = new TextualMedia { Text = "This is a longer text for testing purposes, lorem ipsum dolor est I don't know the rest" };

            // Act
            try
            {
                var result = decomposer.DecomposeText(textSource.Text).Result;
            } catch (AggregateException e)
            {
                throw e.InnerException; // bit hacky but showcases the desired behaviour
            }
        }

        // TODO - this should also throw - the real test mayber needs higher max volume to process?
        [TestMethod]
        public void DecomposeText_WithBijectRequirementNotMet_ShouldThrowException()
        {
            try
            {
                var decomposer = new TextDecomposer(/*100, new WontBijectWillInjectAgent()*/);

                AgentBase agent = new WontBijectWillInjectAgent();
                decomposer.StandardAgent = agent;
                decomposer.HighPerformanceAgent = agent;
                decomposer.FallbackAgent = agent;
                decomposer.MaxVolumeToProcess = 1000;

                var textSource = new TextualMedia { Text = "This is a longer text for testing biject requirements" };

                // Act
                decomposer.DecomposeText(textSource.Text, mustBiject: true).Wait();
                Assert.Fail("Expected an invalid decomposition exception");
            }
            catch (AggregateException ae)
            {
                bool expectedExceptionThrown = false;

                foreach (var exception in ae.InnerExceptions)
                {
                    if (exception is Exception && exception.Message == "Invalid decomposition")
                    {
                        expectedExceptionThrown = true;
                        break;
                    }
                }

                Assert.IsTrue(expectedExceptionThrown, "expected an Invalid Decomposition exception");

            }
        }

        [TestMethod]
        public void DecomposeText_WithInjectRequirementNotMet_ShouldThrowException()
        {
            // Arrange
            try
            {
                var decomposer = new TextDecomposer(/*50, new WontlInjectAgent()*/);

                decomposer.StandardAgent = new WontlInjectAgent();
                decomposer.HighPerformanceAgent = new WontlInjectAgent();
                decomposer.FallbackAgent = new WontlInjectAgent();
                decomposer.MaxVolumeToProcess = 500;

                var textSource = new TextualMedia { Text = "This text is not expected to inject properly" };

                decomposer.DecomposeText(textSource.Text, mustBiject: true).Wait();
                Assert.Fail("Expected an invalid decomposition exception");
            }
            catch (AggregateException ae)
            {
                bool expectedExceptionThrown = false;

                foreach (var exception in ae.InnerExceptions)
                {
                    if (exception is Exception && exception.Message == "Invalid decomposition")
                    {
                        expectedExceptionThrown = true;
                        break;
                    }
                }

                Assert.IsTrue(expectedExceptionThrown, "expected an Invalid decomposition exception");
            }
        }

        [TestMethod]
        public void DecomposeText_WithInjectRequirementNotMetThenMet_ShouldNotThrowException()
        {
            var decomposer = new TextDecomposer(/*50, new WontlInjectAgent()*/);

            decomposer.StandardAgent = new WontlInjectAgent();
            decomposer.HighPerformanceAgent = new WhitespaceDecompositionAgent();
            decomposer.FallbackAgent = new WhitespaceDecompositionAgent();
            decomposer.MaxVolumeToProcess = 500;

            var textSource = new TextualMedia { Text = "This text is not expected to inject properly first time" };

            decomposer.DecomposeText(textSource.Text).Wait();
        }
    }
}

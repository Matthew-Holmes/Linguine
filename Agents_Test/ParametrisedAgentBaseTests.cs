using Agents;
using Helpers;

namespace Agents_Test
{
    [TestClass]
    public class ParametrisedAgentBaseTests
    {
        public class TestAgent : ParametrisedAgentBase
        {
            // This class is just for testing purposes and inherits from ParametrisedAgentBase
            protected override Task<string> GetResponseCore(string prompt)
            {
                throw new NotImplementedException();
            }
        }

        [TestMethod]
        public void ToJon_RunsCorrectly()
        {
            var agent = new TestAgent();
            agent.ContinuousParameters.Add(new Parameter<double>("param1", 1.23, double.MaxValue, 0.0));
            agent.DiscreteParameters.Add(new Parameter<int>("param2", 42, int.MaxValue, 1));
            agent.StringParameters.Add("Key1", "Value1");

            string json = agent.ToJson();

            Assert.IsNotNull(json);
            Assert.IsTrue(json.Contains("param1"));
            Assert.IsTrue(json.Contains("1.23"));
            Assert.IsTrue(json.Contains("0.0"));
            Assert.IsTrue(json.Contains(double.MaxValue.ToString()));
            Assert.IsTrue(json.Contains("param2"));
            Assert.IsTrue(json.Contains("42"));
            Assert.IsTrue(json.Contains("1"));
            Assert.IsTrue(json.Contains(int.MaxValue.ToString()));
            Assert.IsTrue(json.Contains("Key1"));
            Assert.IsTrue(json.Contains("Value1"));
        }

        [TestMethod]
        public void ToJson_IgnoresExtraProperties()
        {
            var agent = new TestAgent();
            agent.ContinuousParameters.Add(new Parameter<double>("param1", 1.23, double.MaxValue, 0.0));
            agent.DiscreteParameters.Add(new Parameter<int>("param2", 42, int.MaxValue, 1));
            agent.StringParameters.Add("Key1", "Value1");

            string json = agent.ToJson();

            Assert.IsNotNull(json);
            Assert.IsFalse(json.Contains(nameof(agent.ConcurrentPromptLog)));
            Assert.IsFalse(json.Contains(nameof(agent.SequentialPromptLog)));
            Assert.IsFalse(json.Contains(nameof(agent.MaxConcurrentResponses)));
        }
        // TODO - round trip serialisation once hashing algo implemented and tested

        [TestMethod]
        public void GetHashCode_SameParametersSameHash()
        {
            var agent1 = new TestAgent();
            agent1.ContinuousParameters.Add(new Parameter<double>("param1", 1.23, double.MaxValue, 0.0));
            agent1.DiscreteParameters.Add(new Parameter<int>("param2", 42, int.MaxValue, 1));
            agent1.StringParameters.Add("Key1", "Value1");

            var agent2 = new TestAgent();
            agent2.ContinuousParameters.Add(new Parameter<double>("param1", 1.23, double.MaxValue, 0.0));
            agent2.DiscreteParameters.Add(new Parameter<int>("param2", 42, int.MaxValue, 1));
            agent2.StringParameters.Add("Key1", "Value1"); 

            int hash1 = agent1.GetHashCode();
            int hash2 = agent2.GetHashCode();

            Assert.AreEqual(hash1, hash2, "Hash codes should be the same for agents with identical parameters.");
        }

        [TestMethod]
        public void GetHashCode_DifferentParametersDifferentHash()
        {
            var agent1 = new TestAgent();
            agent1.ContinuousParameters.Add(new Parameter<double>("param1", 1.23, double.MaxValue, 0.0));
            agent1.DiscreteParameters.Add(new Parameter<int>("param2", 42, int.MaxValue, 1));
            agent1.StringParameters.Add("Key1", "Value1");

            var agent2 = new TestAgent();
            agent2.ContinuousParameters.Add(new Parameter<double>("param1", 2.34, double.MaxValue, 0.0));
            agent2.DiscreteParameters.Add(new Parameter<int>("param2", 42, int.MaxValue, 1));
            agent2.StringParameters.Add("Key1", "Value1");
            // Act
            int hash1 = agent1.GetHashCode();
            int hash2 = agent2.GetHashCode();

            // Assert
            Assert.AreNotEqual(hash1, hash2, "Hash codes should be different for agents with different parameters.");
        }

        [TestMethod]
        public void TestHashCode_AddingAParameterMakesDifferentHash()
        {
            var agent1 = new TestAgent();
            agent1.ContinuousParameters.Add(new Parameter<double>("param1", 1.23, double.MaxValue, 0.0));
            agent1.DiscreteParameters.Add(new Parameter<int>("param2", 42, int.MaxValue, 1));
            agent1.StringParameters.Add("Key1", "Value1");

            var agent2 = new TestAgent();
            agent2.ContinuousParameters.Add(new Parameter<double>("param1", 1.23, double.MaxValue, 0.0));
            agent2.ContinuousParameters.Add(new Parameter<double>("param1b", 1.23, double.MaxValue, 0.0));
            agent2.DiscreteParameters.Add(new Parameter<int>("param2", 42, int.MaxValue, 1));
            agent2.StringParameters.Add("Key1", "Value1");
            // Act
            int hash1 = agent1.GetHashCode();
            int hash2 = agent2.GetHashCode();

            // Assert
            Assert.AreNotEqual(hash1, hash2, "Hash codes should be different for agents with different parameters.");
        }


        [TestMethod]
        public void GetHashCode_ChangingDistributionalInformationLeavesHashAlone()
        {
            var agent1 = new TestAgent();
            agent1.ContinuousParameters.Add(new Parameter<double>("param1", 1.23, 10.0, 0.0));
            agent1.DiscreteParameters.Add(new Parameter<int>("param2", 42, 100, 1));
            agent1.StringParameters.Add("Key1", "Value1");

            var agent2 = new TestAgent();
            agent2.ContinuousParameters.Add(new Parameter<double>("param1", 1.23, double.MaxValue, 0.0));
            agent2.DiscreteParameters.Add(new Parameter<int>("param2", 42, int.MaxValue, 1));
            agent2.StringParameters.Add("Key1", "Value1");
            // Act
            int hash1 = agent1.GetHashCode();
            int hash2 = agent2.GetHashCode();

            // Assert
            Assert.AreEqual(hash1, hash2, "changing distributional information shouldn't affect hashing");
        }


        [TestMethod]
        public void GetHashCode_IsConsistent()
        {
            // Arrange
            var agent = new TestAgent();
            agent.ContinuousParameters.Add(new Parameter<double>("param1", 1.23, 10.0, 0.0));
            agent.DiscreteParameters.Add(new Parameter<int>("param2", 42, 100, 1));
            agent.StringParameters.Add("Key1", "Value1");

            // Act
            int hash1 = agent.GetHashCode();
            int hash2 = agent.GetHashCode();

            // Assert
            Assert.AreEqual(hash1, hash2, "Hash code should be consistent and return the same value for subsequent calls.");
        }
    }
}
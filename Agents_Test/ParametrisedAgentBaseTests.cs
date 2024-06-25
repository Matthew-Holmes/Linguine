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
        public void TestSerialization()
        {
            // Arrange
            var agent = new TestAgent();
            agent.ContinuousParameters.Add(new Parameter<double>("param1", 1.23, double.MaxValue, 0.0));
            agent.DiscreteParameters.Add(new Parameter<int>("param2", 42, int.MaxValue, 1));
            agent.StringParameters.Add("Key1", "Value1");

            // Act
            string json = agent.ToJson();

            // Assert
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
        public void TestSerializationJsonIgnore()
        {
            // Arrange
            var agent = new TestAgent();
            agent.ContinuousParameters.Add(new Parameter<double>("param1", 1.23, double.MaxValue, 0.0));
            agent.DiscreteParameters.Add(new Parameter<int>("param2", 42, int.MaxValue, 1));
            agent.StringParameters.Add("Key1", "Value1");

            // Act
            string json = agent.ToJson();

            // Assert
            Assert.IsNotNull(json);
            Assert.IsFalse(json.Contains(nameof(agent.ConcurrentPromptLog)));
            Assert.IsFalse(json.Contains(nameof(agent.SequentialPromptLog)));
            Assert.IsFalse(json.Contains(nameof(agent.MaxConcurrentResponses)));
        }
        // TODO - round trip serialisation once hashing algo implemented and tested
    }
}
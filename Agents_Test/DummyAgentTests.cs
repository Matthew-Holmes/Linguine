using Agents.DummyAgents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests_Agents
{
    [TestClass]
    public class DummyAgentTests
    {

        [TestMethod]
        public void GetHashCode_DistinctForEach()
        {
            DummyTextDecompositionAgent a1 = new DummyTextDecompositionAgent();
            LowercasingAgent a2 = new LowercasingAgent();
            SentenceDecompositionAgent a3 = new SentenceDecompositionAgent();
            WhitespaceDecompositionAgent a4 = new WhitespaceDecompositionAgent();

            Assert.AreNotEqual(a1.GetHashCode(), a2.GetHashCode());
            Assert.AreNotEqual(a1.GetHashCode(), a3.GetHashCode());
            Assert.AreNotEqual(a1.GetHashCode(), a4.GetHashCode());

            Assert.AreNotEqual(a2.GetHashCode(), a3.GetHashCode());
            Assert.AreNotEqual(a2.GetHashCode(), a4.GetHashCode());

            Assert.AreNotEqual(a3.GetHashCode(), a4.GetHashCode());
        }

        [TestMethod]
        public void GetHashCode_Persists()
        {
            DummyTextDecompositionAgent a1A = new DummyTextDecompositionAgent();
            DummyTextDecompositionAgent a1B = new DummyTextDecompositionAgent();

            LowercasingAgent a2A = new LowercasingAgent();
            LowercasingAgent a2B = new LowercasingAgent();

            SentenceDecompositionAgent a3A = new SentenceDecompositionAgent();
            SentenceDecompositionAgent a3B = new SentenceDecompositionAgent();

            WhitespaceDecompositionAgent a4A = new WhitespaceDecompositionAgent();
            WhitespaceDecompositionAgent a4B = new WhitespaceDecompositionAgent();

            Assert.AreEqual(a1A.GetHashCode(), a1B.GetHashCode());
            Assert.AreEqual(a2A.GetHashCode(), a2B.GetHashCode());
            Assert.AreEqual(a3A.GetHashCode(), a3B.GetHashCode());
            Assert.AreEqual(a4A.GetHashCode(), a4B.GetHashCode());
        }

    }
}

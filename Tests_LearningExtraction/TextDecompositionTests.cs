using Microsoft.VisualStudio.TestTools.UnitTesting;
using LearningExtraction;
using ExternalMedia; // Assuming this namespace contains the definition of TextUnit
using System.Collections.Generic;

namespace Tests_LearningExtraction
{
    [TestClass]
    public class TextDecompositionTests
    {
        [TestMethod]
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Arrange
            TextUnit total = new TextUnit("Parent text");
            List<TextDecomposition> units = new List<TextDecomposition>
            {
                new TextDecomposition(new TextUnit("Child text"), null)
            };

            // Act
            TextDecomposition decomposition = new TextDecomposition(total, units);

            // Assert
            Assert.AreEqual(total, decomposition.Total);
            Assert.AreEqual(units, decomposition.Units);
        }

        [TestMethod]
        public void FromNewLinedString_CreatesCorrectDecomposition()
        {
            // Arrange
            string parent = "Parent text";
            string newLinedDecomposition = "Child text 1\nChild text 2";

            // Act
            TextDecomposition decomposition = TextDecomposition.FromNewLinedString(parent, newLinedDecomposition);

            // Assert
            Assert.AreEqual(parent, decomposition.Total.Text); // Assuming TextUnit has a Content property
            Assert.IsNotNull(decomposition.Units);
            Assert.AreEqual(2, decomposition.Units.Count);
            Assert.AreEqual("Child text 1", decomposition.Units[0].Total.Text);
            Assert.AreEqual("Child text 2", decomposition.Units[1].Total.Text);
        }

        [TestMethod]
        public void FromNewLinedString_EmptyStringCreatesLeaf()
        {
            string parent = "Parent text";
            string newLinedDecomposition = "";

            TextDecomposition decomposition = TextDecomposition.FromNewLinedString(parent, newLinedDecomposition);

            Assert.AreEqual(parent, decomposition.Total.Text);
            Assert.IsNull(decomposition.Units);
        }

        [TestMethod]
        public void FromNewLinedString_NoNewLinesCreatesLeaf()
        {
            string parent = "Parent text";
            string newLinedDecomposition = "Parent text";

            TextDecomposition decomposition = TextDecomposition.FromNewLinedString(parent, newLinedDecomposition);

            Assert.AreEqual(parent, decomposition.Total.Text);
            Assert.IsNull(decomposition.Units);
        }

        [TestMethod]
        public void TestInjects_WithDirectSubstrings_ShouldReturnTrue()
        {
            // Arrange
            var parent = new TextUnit("hello world");
            var child1 = new TextDecomposition(new TextUnit("hello"), null);
            var child2 = new TextDecomposition(new TextUnit(" world"), null);
            var decomposition = new TextDecomposition(parent, new List<TextDecomposition> { child1, child2 });

            // Act
            bool result = decomposition.Injects();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestInjects_WithIndirectSubstrings_ShouldReturnTrue()
        {
            // Arrange
            var parent = new TextUnit("hello world");
            var child1 = new TextDecomposition(new TextUnit("ello"), null);
            var child2 = new TextDecomposition(new TextUnit("world"), null);
            var decomposition = new TextDecomposition(parent, new List<TextDecomposition> { child1, child2 });

            // Act
            bool result = decomposition.Injects();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestInjects_WithNonSubstrings_ShouldReturnFalse()
        {
            // Arrange
            var parent = new TextUnit("hello world");
            var child = new TextDecomposition(new TextUnit("universe"), null);
            var decomposition = new TextDecomposition(parent, new List<TextDecomposition> { child });

            // Act
            bool result = decomposition.Injects();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestBijects_WithExactMatch_ShouldReturnTrue()
        {
            // Arrange
            var parent = new TextUnit("hello");
            var child = new TextDecomposition(new TextUnit("hello"), null);
            var decomposition = new TextDecomposition(parent, new List<TextDecomposition> { child });

            // Act
            bool result = decomposition.Bijects();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestBijects_WithMismatch_ShouldReturnFalse()
        {
            // Arrange
            var parent = new TextUnit("hello world");
            var child = new TextDecomposition(new TextUnit("hello"), null);
            var decomposition = new TextDecomposition(parent, new List<TextDecomposition> { child });

            // Act
            bool result = decomposition.Bijects();

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void TestFlattened_WithNestedUnits_ShouldReturnFlatList()
        {
            // Arrange
            var parent = new TextUnit("hello world");
            var child1 = new TextDecomposition(new TextUnit("hello"), null);
            var child2 = new TextDecomposition(new TextUnit(" world"), new List<TextDecomposition>
            {
                new TextDecomposition(new TextUnit("wor"), null), // Breaking down "world" into "wor"...
                new TextDecomposition(new TextUnit("ld"), null)  // ...and "ld"
            });
            var decomposition = new TextDecomposition(parent, new List<TextDecomposition> { child1, child2 });

            // Act
            var flattened = decomposition.Flattened();

            // Assert
            Assert.AreEqual(3, flattened.Units.Count); // Now we expect 3 units after flattening: "hello", "wor", and "ld"
            Assert.IsNull(flattened.Units[0].Units); // Ensure "hello" is a leaf
            Assert.IsNull(flattened.Units[1].Units); // Ensure "wor" is a leaf
            Assert.IsNull(flattened.Units[2].Units); // Ensure "ld" is a leaf
        }

        [TestMethod]
        public void TestInjects_WithNestedDecompositions_ShouldReturnTrue()
        {
            // Arrange
            var parent = new TextUnit("I like ice cream");
            var child1 = new TextDecomposition(new TextUnit("I like "), null);
            var child2 = new TextDecomposition(new TextUnit("ice cream"), new List<TextDecomposition>
            {
                new TextDecomposition(new TextUnit("ice"), null),
                new TextDecomposition(new TextUnit("cream"), null)
            });
            var decomposition = new TextDecomposition(parent, new List<TextDecomposition> { child1, child2 });

            // Act
            bool result = decomposition.Injects();

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void TestBijects_WithNestedDecompositions_ShouldReturnTrue()
        {
            // Arrange
            var parent = new TextUnit("I like ice cream");
            var child1 = new TextDecomposition(new TextUnit("I like "), null);
            var child2 = new TextDecomposition(new TextUnit("ice cream"), new List<TextDecomposition>
            {
                new TextDecomposition(new TextUnit("ice"), null),
                new TextDecomposition(new TextUnit(" cream"), null)
            });
            var decomposition = new TextDecomposition(parent, new List<TextDecomposition> { child1, child2 });

            // Act
            bool result = decomposition.Bijects();

            // Assert
            Assert.IsTrue(result); // This assumes that the sum of the lengths of all leaves exactly matches the length of the parent.
        }

        [TestMethod]
        public void TestBijects_WithOverlappingNestedDecompositions_ShouldReturnFalse()
        {
            // Arrange
            var parent = new TextUnit("I like ice cream");
            // Intentionally creating an overlap where "ice" is included in both child1 and child2
            var child1 = new TextDecomposition(new TextUnit("I like ice"), null);
            var child2 = new TextDecomposition(new TextUnit("ice cream"), new List<TextDecomposition>
            {
                new TextDecomposition(new TextUnit("ice"), null),
                new TextDecomposition(new TextUnit(" cream"), null)
            });
            var decomposition = new TextDecomposition(parent, new List<TextDecomposition> { child1, child2 });

            // Act
            bool result = decomposition.Bijects();

            // Assert
            Assert.IsFalse(result); // This checks that overlapping decompositions do not falsely report as bijecting.
        }

        [TestMethod]
        public void TestInjects_WithOverlappingNestedDecompositions_ShouldReturnFalse()
        {
            // Arrange
            var parent = new TextUnit("I like ice cream");
            // Intentionally creating an overlap where "ice" is included in both child1 and child2
            var child1 = new TextDecomposition(new TextUnit("I like ice"), null);
            var child2 = new TextDecomposition(new TextUnit("ice cream"), new List<TextDecomposition>
            {
                new TextDecomposition(new TextUnit("ice"), null),
                new TextDecomposition(new TextUnit(" cream"), null)
            });
            var decomposition = new TextDecomposition(parent, new List<TextDecomposition> { child1, child2 });

            // Act
            bool result = decomposition.Injects();

            // Assert
            Assert.IsFalse(result); // This checks that overlapping decompositions do not falsely report as bijecting.
        }

    }
}

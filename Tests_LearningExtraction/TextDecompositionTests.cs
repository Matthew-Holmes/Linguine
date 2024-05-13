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
            Assert.AreEqual(units, decomposition.Decomposition);
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
            Assert.IsNotNull(decomposition.Decomposition);
            Assert.AreEqual(2, decomposition.Decomposition.Count);
            Assert.AreEqual("Child text 1", decomposition.Decomposition[0].Total.Text);
            Assert.AreEqual("Child text 2", decomposition.Decomposition[1].Total.Text);
        }

        [TestMethod]
        public void FromNewLinedString_EmptyStringCreatesLeaf()
        {
            string parent = "Parent text";
            string newLinedDecomposition = "";

            TextDecomposition decomposition = TextDecomposition.FromNewLinedString(parent, newLinedDecomposition);

            Assert.AreEqual(parent, decomposition.Total.Text);
            Assert.IsNull(decomposition.Decomposition);
        }

        [TestMethod]
        public void FromNewLinedString_NoNewLinesCreatesLeaf()
        {
            string parent = "Parent text";
            string newLinedDecomposition = "Parent text";

            TextDecomposition decomposition = TextDecomposition.FromNewLinedString(parent, newLinedDecomposition);

            Assert.AreEqual(parent, decomposition.Total.Text);
            Assert.IsNull(decomposition.Decomposition);
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
            Assert.AreEqual(3, flattened.Decomposition.Count); // Now we expect 3 units after flattening: "hello", "wor", and "ld"
            Assert.IsNull(flattened.Decomposition[0].Decomposition); // Ensure "hello" is a leaf
            Assert.IsNull(flattened.Decomposition[1].Decomposition); // Ensure "wor" is a leaf
            Assert.IsNull(flattened.Decomposition[2].Decomposition); // Ensure "ld" is a leaf
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

        [TestMethod]
        public void Copy_ReturnsNewInstanceWithSameTotalAndUnits()
        {
            // Arrange
            TextUnit total = new TextUnit("Unit1Unit2");
            List<TextDecomposition> units = new List<TextDecomposition>
            {
                new TextDecomposition(new TextUnit("Unit1"), null),
                new TextDecomposition(new TextUnit("Unit2"), null)
            };
            TextDecomposition original = new TextDecomposition(total, units);

            // Act
            TextDecomposition copy = original.Copy();

            // Assert
            Assert.AreNotSame(original, copy);
            Assert.AreEqual(original.Total.Text, copy.Total.Text);
            Assert.AreEqual(original.Decomposition.Count, copy.Decomposition.Count);
            for (int i = 0; i < original.Decomposition.Count; i++)
            {
                Assert.AreEqual(original.Decomposition[i].Total.Text, copy.Decomposition[i].Total.Text);
            }
        }

        [TestMethod]
        public void Copy_ReturnsNewInstanceWithIndependentUnits()
        {
            // Arrange
            TextUnit total = new TextUnit("Unit1Unit2");
            List<TextDecomposition> units = new List<TextDecomposition>
            {
                new TextDecomposition(new TextUnit("Unit1"), null),
                new TextDecomposition(new TextUnit("Unit2"), null)
            };
            TextDecomposition original = new TextDecomposition(total, units);

            // Act
            TextDecomposition copy = original.Copy();

            // Modify the units in the original
            original.Decomposition[0] = new TextDecomposition(new TextUnit("Modified"), null);

            // Assert
            Assert.AreNotEqual(original.Decomposition[0].Total.Text, copy.Decomposition[0].Total.Text);
        }

        [TestMethod]
        public void Copy_ReturnsCopyWhenLeaf()
        {
            // Arrange
            TextUnit total = new TextUnit("Test");
            TextDecomposition original = new TextDecomposition(total, null);

            // Act
            TextDecomposition copy = original.Copy();

            // Assert
            Assert.IsNull(copy.Decomposition);
        }

        [TestMethod]
        public void Copy_ReturnsNewInstanceWithRecursiveUnits()
        {
            // Arrange
            var parent = new TextUnit("hello world");
            var child1 = new TextDecomposition(new TextUnit("hello"), null);
            var child2 = new TextDecomposition(new TextUnit(" world"), new List<TextDecomposition>
                {
                    new TextDecomposition(new TextUnit("wor"), null), // Breaking down "world" into "wor"...
                    new TextDecomposition(new TextUnit("ld"), null)  // ...and "ld"
                });
            var original = new TextDecomposition(parent, new List<TextDecomposition> { child1, child2 });

            // Act
            var copy = original.Copy();

            // Assert
            Assert.AreNotSame(original, copy);
            Assert.AreEqual(original.Total.Text, copy.Total.Text);
            Assert.AreEqual(original.Decomposition.Count, copy.Decomposition.Count);

            // Check recursively for nested units
            Assert.AreNotSame(original.Decomposition[0], copy.Decomposition[0]);
            Assert.AreEqual(original.Decomposition[0].Total.Text, copy.Decomposition[0].Total.Text);
            Assert.IsNull(copy.Decomposition[0].Decomposition); // Ensure "hello" is a leaf

            Assert.AreNotSame(original.Decomposition[1], copy.Decomposition[1]);
            Assert.AreEqual(original.Decomposition[1].Total.Text, copy.Decomposition[1].Total.Text);
            Assert.IsNotNull(copy.Decomposition[1].Decomposition); // Ensure "world" has nested units
            Assert.AreEqual(original.Decomposition[1].Decomposition.Count, copy.Decomposition[1].Decomposition.Count);

            // Check recursively for nested units of "world"
            Assert.AreNotSame(original.Decomposition[1].Decomposition[0], copy.Decomposition[1].Decomposition[0]);
            Assert.AreEqual(original.Decomposition[1].Decomposition[0].Total.Text, copy.Decomposition[1].Decomposition[0].Total.Text);
            Assert.IsNull(copy.Decomposition[1].Decomposition[0].Decomposition); // Ensure "wor" is a leaf

            Assert.AreNotSame(original.Decomposition[1].Decomposition[1], copy.Decomposition[1].Decomposition[1]);
            Assert.AreEqual(original.Decomposition[1].Decomposition[1].Total.Text, copy.Decomposition[1].Decomposition[1].Total.Text);
            Assert.IsNull(copy.Decomposition[1].Decomposition[1].Decomposition); // Ensure "ld" is a leaf
        }

    }
}

using Microsoft.VisualStudio.TestTools.UnitTesting;
using LearningExtraction;
using ExternalMedia; // Assuming this namespace contains the definition of TextUnit
using System.Collections.Generic;
using Newtonsoft.Json;
using System;

namespace Tests_LearningExtraction
{
    [TestClass]
    public class TextDecompositionTests
    {
        [TestMethod]
        public void Constructor_InitializesPropertiesCorrectly()
        {
            // Arrange
            String total = new String("Parent text");
            List<TextDecomposition> units = new List<TextDecomposition>
            {
                new TextDecomposition(new String("Child text"), null)
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
            Assert.AreEqual(parent, decomposition.Total); // Assuming TextUnit has a Content property
            Assert.IsNotNull(decomposition.Decomposition);
            Assert.AreEqual(2, decomposition.Decomposition.Count);
            Assert.AreEqual("Child text 1", decomposition.Decomposition[0].Total);
            Assert.AreEqual("Child text 2", decomposition.Decomposition[1].Total);
        }

        [TestMethod]
        public void FromNewLinedString_EmptyStringCreatesLeaf()
        {
            string parent = "Parent text";
            string newLinedDecomposition = "";

            TextDecomposition decomposition = TextDecomposition.FromNewLinedString(parent, newLinedDecomposition);

            Assert.AreEqual(parent, decomposition.Total);
            Assert.IsNull(decomposition.Decomposition);
        }

        [TestMethod]
        public void FromNewLinedString_NoNewLinesCreatesLeaf()
        {
            string parent = "Parent text";
            string newLinedDecomposition = "Parent text";

            TextDecomposition decomposition = TextDecomposition.FromNewLinedString(parent, newLinedDecomposition);

            Assert.AreEqual(parent, decomposition.Total);
            Assert.IsNull(decomposition.Decomposition);
        }

        [TestMethod]
        public void TestInjects_WithDirectSubstrings_ShouldReturnTrue()
        {
            // Arrange
            var parent = new String("hello world");
            var child1 = new TextDecomposition(new String("hello"), null);
            var child2 = new TextDecomposition(new String(" world"), null);
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
            var parent = new String("hello world");
            var child1 = new TextDecomposition(new String("ello"), null);
            var child2 = new TextDecomposition(new String("world"), null);
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
            var parent = new String("hello world");
            var child = new TextDecomposition(new String("universe"), null);
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
            var parent = new String("hello");
            var child = new TextDecomposition(new String("hello"), null);
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
            var parent = new String("hello world");
            var child = new TextDecomposition(new String("hello"), null);
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
            var parent = new String("hello world");
            var child1 = new TextDecomposition(new String("hello"), null);
            var child2 = new TextDecomposition(new String(" world"), new List<TextDecomposition>
            {
                new TextDecomposition(new String("wor"), null), // Breaking down "world" into "wor"...
                new TextDecomposition(new String("ld"), null)  // ...and "ld"
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
            var parent = new String("I like ice cream");
            var child1 = new TextDecomposition(new String("I like "), null);
            var child2 = new TextDecomposition(new String("ice cream"), new List<TextDecomposition>
            {
                new TextDecomposition(new String("ice"), null),
                new TextDecomposition(new String("cream"), null)
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
            var parent = new String("I like ice cream");
            var child1 = new TextDecomposition(new String("I like "), null);
            var child2 = new TextDecomposition(new String("ice cream"), new List<TextDecomposition>
            {
                new TextDecomposition(new String("ice"), null),
                new TextDecomposition(new String(" cream"), null)
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
            var parent = new String("I like ice cream");
            // Intentionally creating an overlap where "ice" is included in both child1 and child2
            var child1 = new TextDecomposition(new String("I like ice"), null);
            var child2 = new TextDecomposition(new String("ice cream"), new List<TextDecomposition>
            {
                new TextDecomposition(new String("ice"), null),
                new TextDecomposition(new String(" cream"), null)
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
            var parent = new String("I like ice cream");
            // Intentionally creating an overlap where "ice" is included in both child1 and child2
            var child1 = new TextDecomposition(new String("I like ice"), null);
            var child2 = new TextDecomposition(new String("ice cream"), new List<TextDecomposition>
            {
                new TextDecomposition(new String("ice"), null),
                new TextDecomposition(new String(" cream"), null)
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
            String total = new String("Unit1Unit2");
            List<TextDecomposition> units = new List<TextDecomposition>
            {
                new TextDecomposition(new String("Unit1"), null),
                new TextDecomposition(new String("Unit2"), null)
            };
            TextDecomposition original = new TextDecomposition(total, units);

            // Act
            TextDecomposition copy = original.Copy();

            // Assert
            Assert.AreNotSame(original, copy);
            Assert.AreEqual(original.Total, copy.Total);
            Assert.AreEqual(original.Decomposition.Count, copy.Decomposition.Count);
            for (int i = 0; i < original.Decomposition.Count; i++)
            {
                Assert.AreEqual(original.Decomposition[i].Total, copy.Decomposition[i].Total);
            }
        }

        [TestMethod]
        public void Copy_ReturnsNewInstanceWithIndependentUnits()
        {
            // Arrange
            String total = new String("Unit1Unit2");
            List<TextDecomposition> units = new List<TextDecomposition>
            {
                new TextDecomposition(new String("Unit1"), null),
                new TextDecomposition(new String("Unit2"), null)
            };
            TextDecomposition original = new TextDecomposition(total, units);

            // Act
            TextDecomposition copy = original.Copy();

            // Modify the units in the original
            original.Decomposition[0] = new TextDecomposition(new String("Modified"), null);

            // Assert
            Assert.AreNotEqual(original.Decomposition[0].Total, copy.Decomposition[0].Total);
        }

        [TestMethod]
        public void Copy_ReturnsCopyWhenLeaf()
        {
            // Arrange
            String total = new String("Test");
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
            var parent = new String("hello world");
            var child1 = new TextDecomposition(new String("hello"), null);
            var child2 = new TextDecomposition(new String(" world"), new List<TextDecomposition>
                {
                    new TextDecomposition(new String("wor"), null), // Breaking down "world" into "wor"...
                    new TextDecomposition(new String("ld"), null)  // ...and "ld"
                });
            var original = new TextDecomposition(parent, new List<TextDecomposition> { child1, child2 });

            // Act
            var copy = original.Copy();

            // Assert
            Assert.AreNotSame(original, copy);
            Assert.AreEqual(original.Total, copy.Total);
            Assert.AreEqual(original.Decomposition.Count, copy.Decomposition.Count);

            // Check recursively for nested units
            Assert.AreNotSame(original.Decomposition[0], copy.Decomposition[0]);
            Assert.AreEqual(original.Decomposition[0].Total, copy.Decomposition[0].Total);
            Assert.IsNull(copy.Decomposition[0].Decomposition); // Ensure "hello" is a leaf

            Assert.AreNotSame(original.Decomposition[1], copy.Decomposition[1]);
            Assert.AreEqual(original.Decomposition[1].Total, copy.Decomposition[1].Total);
            Assert.IsNotNull(copy.Decomposition[1].Decomposition); // Ensure "world" has nested units
            Assert.AreEqual(original.Decomposition[1].Decomposition.Count, copy.Decomposition[1].Decomposition.Count);

            // Check recursively for nested units of "world"
            Assert.AreNotSame(original.Decomposition[1].Decomposition[0], copy.Decomposition[1].Decomposition[0]);
            Assert.AreEqual(original.Decomposition[1].Decomposition[0].Total, copy.Decomposition[1].Decomposition[0].Total);
            Assert.IsNull(copy.Decomposition[1].Decomposition[0].Decomposition); // Ensure "wor" is a leaf

            Assert.AreNotSame(original.Decomposition[1].Decomposition[1], copy.Decomposition[1].Decomposition[1]);
            Assert.AreEqual(original.Decomposition[1].Decomposition[1].Total, copy.Decomposition[1].Decomposition[1].Total);
            Assert.IsNull(copy.Decomposition[1].Decomposition[1].Decomposition); // Ensure "ld" is a leaf
        }

        [TestMethod]
        public void JSONSerialize_NestedDecompositionRuns()
        {
            // Arrange
            var parent = new String("hello world");
            var child1 = new TextDecomposition(new String("hello"), null);
            var child2 = new TextDecomposition(new String(" world"), new List<TextDecomposition>
                {
                    new TextDecomposition(new String("wor"), null), // Breaking down "world" into "wor"...
                    new TextDecomposition(new String("ld"), null)  // ...and "ld"
                });
            var td = new TextDecomposition(parent, new List<TextDecomposition> { child1, child2 });

            // keeps the string length minimal

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(td, settings);

        }

        [TestMethod]
        public void JSONDeSerialize_NestedDecompositionRuns()
        {
            // Arrange
            var parent = new String("hello world");
            var child1 = new TextDecomposition(new String("hello"), null);
            var child2 = new TextDecomposition(new String(" world"), new List<TextDecomposition>
                {
                    new TextDecomposition(new String("wor"), null), // Breaking down "world" into "wor"...
                    new TextDecomposition(new String("ld"), null)  // ...and "ld"
                });
            var td = new TextDecomposition(parent, new List<TextDecomposition> { child1, child2 });

            // keeps the string length minimal

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(td, settings);

            TextDecomposition? tdBack = JsonConvert.DeserializeObject<TextDecomposition>(json, settings);

        }

        [TestMethod]
        public void JSONDeSerialize_NestedDecompositionIsNotNull()
        {
            // Arrange
            var parent = new String("hello world");
            var child1 = new TextDecomposition(new String("hello"), null);
            var child2 = new TextDecomposition(new String(" world"), new List<TextDecomposition>
                {
                    new TextDecomposition(new String("wor"), null), // Breaking down "world" into "wor"...
                    new TextDecomposition(new String("ld"), null)  // ...and "ld"
                });
            var td = new TextDecomposition(parent, new List<TextDecomposition> { child1, child2 });

            // keeps the string length minimal

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(td, settings);

            TextDecomposition? tdBack = JsonConvert.DeserializeObject<TextDecomposition>(json, settings);

            Assert.IsNotNull(tdBack);

        }


        [TestMethod]
        public void JSONDeSerialize_NestedDecompositionHasSameDecomposition()
        {
            // Arrange
            var parent = new String("hello world");
            var child1 = new TextDecomposition(new String("hello"), null);
            var child2 = new TextDecomposition(new String(" world"), new List<TextDecomposition>
                {
                    new TextDecomposition(new String("wor"), null), // Breaking down "world" into "wor"...
                    new TextDecomposition(new String("ld"), null)  // ...and "ld"
                });
            var original = new TextDecomposition(parent, new List<TextDecomposition> { child1, child2 });

            // keeps the string length minimal

            JsonSerializerSettings settings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            string json = JsonConvert.SerializeObject(original, settings);

            TextDecomposition? copy = JsonConvert.DeserializeObject<TextDecomposition>(json, settings);

            Assert.IsNotNull(copy);

            // same checks as the test for copy
            // Assert
            Assert.AreNotSame(original, copy);
            Assert.AreEqual(original.Total, copy.Total);
            Assert.AreEqual(original.Decomposition.Count, copy.Decomposition.Count);

            // Check recursively for nested units
            Assert.AreNotSame(original.Decomposition[0], copy.Decomposition[0]);
            Assert.AreEqual(original.Decomposition[0].Total, copy.Decomposition[0].Total);
            Assert.IsNull(copy.Decomposition[0].Decomposition); // Ensure "hello" is a leaf

            Assert.AreNotSame(original.Decomposition[1], copy.Decomposition[1]);
            Assert.AreEqual(original.Decomposition[1].Total, copy.Decomposition[1].Total);
            Assert.IsNotNull(copy.Decomposition[1].Decomposition); // Ensure "world" has nested units
            Assert.AreEqual(original.Decomposition[1].Decomposition.Count, copy.Decomposition[1].Decomposition.Count);

            // Check recursively for nested units of "world"
            Assert.AreNotSame(original.Decomposition[1].Decomposition[0], copy.Decomposition[1].Decomposition[0]);
            Assert.AreEqual(original.Decomposition[1].Decomposition[0].Total, copy.Decomposition[1].Decomposition[0].Total);
            Assert.IsNull(copy.Decomposition[1].Decomposition[0].Decomposition); // Ensure "wor" is a leaf

            Assert.AreNotSame(original.Decomposition[1].Decomposition[1], copy.Decomposition[1].Decomposition[1]);
            Assert.AreEqual(original.Decomposition[1].Decomposition[1].Total, copy.Decomposition[1].Decomposition[1].Total);
            Assert.IsNull(copy.Decomposition[1].Decomposition[1].Decomposition); // Ensure "ld" is a leaf

        }


    }
}

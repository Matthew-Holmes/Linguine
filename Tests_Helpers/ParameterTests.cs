using Helpers;

namespace Tests_Helpers
{
    [TestClass]
    public class ParameterTests
    {
        [TestMethod]
        public void FullConstructor_ValidParameters_ShouldSetPropertiesInt()
        {
            // Arrange
            string name = "TestParameter";
            int value = 5;
            int upperBound = 10;
            int lowerBound = 1;
            int softUpperBound = 8;
            int softLowerBound = 3;

            // Act
            Parameter<int> parameter = new Parameter<int>(name, value, upperBound, lowerBound, softUpperBound, softLowerBound);

            // Assert
            Assert.AreEqual(name, parameter.Name);
            Assert.AreEqual(value, parameter.Value);
            Assert.AreEqual(upperBound, parameter.UpperBound);
            Assert.AreEqual(lowerBound, parameter.LowerBound);
            Assert.AreEqual(softUpperBound, parameter.SoftUpperBound);
            Assert.AreEqual(softLowerBound, parameter.SoftLowerBound);
        }

        [TestMethod]
        public void Constructor_ValidParameters_ShouldSetPropertiesInt()
        {
            // Arrange
            string name = "TestParameter";
            int value = 5;
            int upperBound = 10;
            int lowerBound = 1;

            // Act
            Parameter<int> parameter = new Parameter<int>(name, value, upperBound, lowerBound);

            // Assert
            Assert.AreEqual(name, parameter.Name);
            Assert.AreEqual(value, parameter.Value);
            Assert.AreEqual(upperBound, parameter.UpperBound);
            Assert.AreEqual(lowerBound, parameter.LowerBound);
            Assert.AreEqual(upperBound, parameter.SoftUpperBound);
            Assert.AreEqual(lowerBound, parameter.SoftLowerBound);
        }

        [TestMethod]
        public void FullConstructor_ValidParameters_ShouldSetPropertiesDouble()
        {
            // Arrange
            string name = "TestParameter";
            double value = 5.0;
            double upperBound = 10.0;
            double lowerBound = 1.0;
            double softUpperBound = 8.0;
            double softLowerBound = 3.0;

            // Act
            Parameter<double> parameter = new Parameter<double>(name, value, upperBound, lowerBound, softUpperBound, softLowerBound);

            // Assert
            Assert.AreEqual(name, parameter.Name);
            Assert.AreEqual(value, parameter.Value);
            Assert.AreEqual(upperBound, parameter.UpperBound);
            Assert.AreEqual(lowerBound, parameter.LowerBound);
            Assert.AreEqual(softUpperBound, parameter.SoftUpperBound);
            Assert.AreEqual(softLowerBound, parameter.SoftLowerBound);
        }

        [TestMethod]
        public void Constructor_ValidParameters_ShouldSetPropertiesDouble()
        {
            // Arrange
            string name = "TestParameter";
            double value = 5.0;
            double upperBound = 10.0;
            double lowerBound = 1.0;

            // Act
            Parameter<double> parameter = new Parameter<double>(name, value, upperBound, lowerBound);

            // Assert
            Assert.AreEqual(name, parameter.Name);
            Assert.AreEqual(value, parameter.Value);
            Assert.AreEqual(upperBound, parameter.UpperBound);
            Assert.AreEqual(lowerBound, parameter.LowerBound);
            Assert.AreEqual(upperBound, parameter.SoftUpperBound);
            Assert.AreEqual(lowerBound, parameter.SoftLowerBound);
        }

        [TestMethod]
        public void FullConstructor_ValidParameters_ShouldSetPropertiesDecimal()
        {
            // Arrange
            string name = "TestParameter";
            decimal value = 5.0m;
            decimal upperBound = 10.0m;
            decimal lowerBound = 1.0m;
            decimal softUpperBound = 8.0m;
            decimal softLowerBound = 3.0m;

            // Act
            Parameter<decimal> parameter = new Parameter<decimal>(name, value, upperBound, lowerBound, softUpperBound, softLowerBound);

            // Assert
            Assert.AreEqual(name, parameter.Name);
            Assert.AreEqual(value, parameter.Value);
            Assert.AreEqual(upperBound, parameter.UpperBound);
            Assert.AreEqual(lowerBound, parameter.LowerBound);
            Assert.AreEqual(softUpperBound, parameter.SoftUpperBound);
            Assert.AreEqual(softLowerBound, parameter.SoftLowerBound);
        }

        [TestMethod]
        public void Constructor_ValidParameters_ShouldSetPropertiesDecimal()
        { 
            // Arrange
            string name = "TestParameter";
            decimal value = 5.0m;
            decimal upperBound = 10.0m;
            decimal lowerBound = 1.0m;

            // Act
            Parameter<decimal> parameter = new Parameter<decimal>(name, value, upperBound, lowerBound);

            // Assert
            Assert.AreEqual(name, parameter.Name);
            Assert.AreEqual(value, parameter.Value);
            Assert.AreEqual(upperBound, parameter.UpperBound);
            Assert.AreEqual(lowerBound, parameter.LowerBound);
            Assert.AreEqual(upperBound, parameter.SoftUpperBound);
            Assert.AreEqual(lowerBound, parameter.SoftLowerBound);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void Constructor_ValueExceedsUpperBound_ShouldThrowExceptionInt()
        {
            // Arrange
            string name = "TestParameter";
            int value = 11;
            int upperBound = 10;
            int lowerBound = 1;

            // Act & Assert
            Parameter<int> parameter = new Parameter<int>(name, value, upperBound, lowerBound);
        }

        public void SetValue_WithinBounds_ShouldUpdateValueInt()
        {
            // Arrange
            Parameter<int> parameter = new Parameter<int>("Test", 5, 10, 1);
            int newValue = 7;

            // Act
            parameter.Value = newValue;

            // Assert
            Assert.AreEqual(newValue, parameter.Value);
        }

        [TestMethod]
        public void SetValue_ExceedsUpperBounds_ShouldNotUpdateValueInt()
        {
            // Arrange
            Parameter<int> parameter = new Parameter<int>("Test", 5, 10, 1);
            int originalValue = parameter.Value;
            int newValue = 11; // Exceeds upper bound

            // Act
            parameter.Value = newValue;

            // Assert
            Assert.AreEqual(originalValue, parameter.Value);
        }

        [TestMethod]
        public void SetValue_ExceedsLowerBounds_ShouldNotUpdateValueInt()
        {
            // Arrange
            Parameter<int> parameter = new Parameter<int>("Test", 5, 10, 1);
            int originalValue = parameter.Value;
            int newValue = -1; // Exceeds lower bound

            // Act
            parameter.Value = newValue;

            // Assert
            Assert.AreEqual(originalValue, parameter.Value);
        }

        public void SetSLB_WithinBounds_ShouldUpdateValueInt()
        {
            // Arrange
            Parameter<int> parameter = new Parameter<int>("Test", 5, 10, 1);
            int newSLB = 7;

            // Act
            parameter.SoftLowerBound = newSLB;

            // Assert
            Assert.AreEqual(newSLB, parameter.SoftUpperBound);
        }

        [TestMethod]
        public void SetSLB_ExceedsUpperBounds_ShouldNotUpdateValueInt()
        {
            // Arrange
            Parameter<int> parameter = new Parameter<int>("Test", 5, 10, 1);
            int originalSLB = parameter.SoftLowerBound;
            int newValue = 11; // Exceeds upper bound

            // Act
            parameter.SoftLowerBound = newValue;

            // Assert
            Assert.AreEqual(originalSLB, parameter.SoftLowerBound);
        }

        [TestMethod]
        public void SetValueSLB_ExceedsLowerBounds_ShouldNotUpdateValueInt()
        {
            // Arrange
            Parameter<int> parameter = new Parameter<int>("Test", 5, 10, 1);
            int originalSLB = parameter.SoftLowerBound;
            int newValue = -1; // Exceeds lower bound

            // Act
            parameter.SoftLowerBound = newValue;

            // Assert
            Assert.AreEqual(originalSLB, parameter.SoftLowerBound);
        }

        public void SetSUB_WithinBounds_ShouldUpdateValueInt()
        {
            // Arrange
            Parameter<int> parameter = new Parameter<int>("Test", 5, 10, 1);
            int newSUB = 7;

            // Act
            parameter.SoftUpperBound = newSUB;

            // Assert
            Assert.AreEqual(newSUB, parameter.SoftUpperBound);
        }

        [TestMethod]
        public void SetSUB_ExceedsUpperBounds_ShouldNotUpdateValueInt()
        {
            // Arrange
            Parameter<int> parameter = new Parameter<int>("Test", 5, 10, 1);
            int originalSUB = parameter.SoftUpperBound;
            int newValue = 11; // Exceeds upper bound

            // Act
            parameter.SoftUpperBound = newValue;

            // Assert
            Assert.AreEqual(originalSUB, parameter.SoftUpperBound);
        }

        [TestMethod]
        public void SetValueSUB_ExceedsLowerBounds_ShouldNotUpdateValueInt()
        {
            // Arrange
            Parameter<int> parameter = new Parameter<int>("Test", 5, 10, 1);
            int originalSUB = parameter.SoftUpperBound;
            int newValue = -1; // Exceeds lower bound

            // Act
            parameter.SoftUpperBound = newValue;

            // Assert
            Assert.AreEqual(originalSUB, parameter.SoftUpperBound);
        }

    }
}

using System;
using NUnit.Framework;

namespace Game.Content.Tests
{
    public class NumericValidationTests
    {
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(float.NegativeInfinity)]
        public void ValidateFinite_RejectsNonFiniteValues(float value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => NumericValidation.ValidateFinite(value, "value"));
        }

        [TestCase(0f)]
        [TestCase(-3.5f)]
        [TestCase(1e30f)]
        public void ValidateFinite_AcceptsAnyFiniteValue(float value)
        {
            Assert.DoesNotThrow(() => NumericValidation.ValidateFinite(value, "value"));
        }

        [TestCase(0f)]
        [TestCase(-0.0001f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void ValidatePositive_RejectsZeroNegativeAndNonFinite(float value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => NumericValidation.ValidatePositive(value, "value"));
        }

        [Test]
        public void ValidatePositive_AcceptsSmallPositive()
        {
            Assert.DoesNotThrow(() => NumericValidation.ValidatePositive(0.0001f, "value"));
        }

        [TestCase(-0.0001f)]
        [TestCase(float.NaN)]
        [TestCase(float.NegativeInfinity)]
        public void ValidateNonNegative_RejectsNegativeAndNonFinite(float value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => NumericValidation.ValidateNonNegative(value, "value"));
        }

        [Test]
        public void ValidateNonNegative_AcceptsZero()
        {
            Assert.DoesNotThrow(() => NumericValidation.ValidateNonNegative(0f, "value"));
        }

        [TestCase(-1f)]
        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        public void ValidateNonNegativeFinite_RejectsNegativeAndNonFinite(float value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => NumericValidation.ValidateNonNegativeFinite(value, "value"));
        }

        [Test]
        public void ValidateNonNegativeFinite_AcceptsZeroAndPositive()
        {
            Assert.DoesNotThrow(() => NumericValidation.ValidateNonNegativeFinite(0f, "value"));
            Assert.DoesNotThrow(() => NumericValidation.ValidateNonNegativeFinite(12f, "value"));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void ValidateCount_RejectsZeroAndNegative(int value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => NumericValidation.ValidateCount(value, "value"));
        }

        [Test]
        public void ValidateCount_AcceptsOne()
        {
            Assert.DoesNotThrow(() => NumericValidation.ValidateCount(1, "value"));
        }

        [TestCase(-0.01f)]
        [TestCase(1.01f)]
        [TestCase(float.NaN)]
        public void ValidateRange_RejectsOutsideOrNonFinite(float value)
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => NumericValidation.ValidateRange(value, 0f, 1f, "value"));
        }

        [TestCase(0f)]
        [TestCase(0.5f)]
        [TestCase(1f)]
        public void ValidateRange_AcceptsInclusiveBounds(float value)
        {
            Assert.DoesNotThrow(() => NumericValidation.ValidateRange(value, 0f, 1f, "value"));
        }

        [Test]
        public void Failure_CarriesParameterNameAndCustomMessage()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                NumericValidation.ValidatePositive(0f, "threshold", "Thresholds must be greater than zero."));

            Assert.AreEqual("threshold", exception.ParamName);
            StringAssert.Contains("Thresholds must be greater than zero.", exception.Message);
        }

        [Test]
        public void Failure_UsesDefaultMessageWhenNoneGiven()
        {
            var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
                NumericValidation.ValidateNonNegative(-1f, "amount"));

            StringAssert.Contains("Value cannot be negative.", exception.Message);
        }
    }
}

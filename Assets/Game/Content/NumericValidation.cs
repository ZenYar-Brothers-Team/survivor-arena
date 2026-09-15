using System;

namespace Game.Content
{
    public static class NumericValidation
    {
        public static void ValidateFinite(float value, string parameterName, string message = "Value must be finite.")
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(parameterName, message);
        }

        public static void ValidatePositive(float value, string parameterName, string message = "Value must be greater than zero.")
        {
            ValidateFinite(value, parameterName);
            if (value <= 0f)
                throw new ArgumentOutOfRangeException(parameterName, message);
        }

        public static void ValidateNonNegative(float value, string parameterName, string message = "Value cannot be negative.")
        {
            ValidateFinite(value, parameterName);
            if (value < 0f)
                throw new ArgumentOutOfRangeException(parameterName, message);
        }

        public static void ValidateNonNegativeFinite(float value, string parameterName, string message = "Value must be finite and non-negative.")
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(parameterName, message);
        }

        public static void ValidateCount(int value, string parameterName, string message = "Count must be greater than zero.")
        {
            if (value <= 0)
                throw new ArgumentOutOfRangeException(parameterName, message);
        }

        public static void ValidateRange(float value, float minimum, float maximum, string parameterName)
        {
            ValidateFinite(value, parameterName);
            if (value < minimum || value > maximum)
                throw new ArgumentOutOfRangeException(parameterName, $"Value must be between {minimum} and {maximum}.");
        }
    }
}

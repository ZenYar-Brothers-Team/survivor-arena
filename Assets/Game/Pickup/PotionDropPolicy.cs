using System;
using Game.Content;
namespace Game.Pickup
{
    /// <summary>DECISION-0033: first present enemy/field/global value, including zero, then relative multiplier and probability cap.</summary>
    public static class PotionDropPolicy
    {
        public static float Chance(float global, float? field, float? enemy, float multiplier)
        {
            NumericValidation.ValidateRange(global, 0, 1, nameof(global));
            if (field.HasValue) NumericValidation.ValidateRange(field.Value, 0, 1, nameof(field));
            if (enemy.HasValue) NumericValidation.ValidateRange(enemy.Value, 0, 1, nameof(enemy));
            NumericValidation.ValidateNonNegative(multiplier, nameof(multiplier));
            return (float)Math.Min(1d, (double)(enemy ?? field ?? global) * multiplier);
        }
        public static bool Roll(float chance, double sample)
        {
            NumericValidation.ValidateRange(chance, 0, 1, nameof(chance));
            if (double.IsNaN(sample) || sample < 0 || sample >= 1) throw new ArgumentOutOfRangeException(nameof(sample));
            return sample < chance;
        }
    }
}

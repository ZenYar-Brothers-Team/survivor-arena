using System;
using Game.Content;

namespace Game.Enemy
{
    /// <summary>Pure geometry for the opening spawn (DECISION-0057): the point where a ray
    /// from the view center at <c>angle</c> leaves the view rectangle grown by
    /// <c>margin</c>, so an enemy starts just outside the screen on every side.</summary>
    public static class WaveScreenEdgePlacement
    {
        public static void Offset(double angle, float halfWidth, float halfHeight, float margin,
            out float x, out float y)
        {
            NumericValidation.ValidatePositive(halfWidth, nameof(halfWidth));
            NumericValidation.ValidatePositive(halfHeight, nameof(halfHeight));
            NumericValidation.ValidateNonNegativeFinite(margin, nameof(margin));

            var dx = Math.Cos(angle);
            var dy = Math.Sin(angle);
            var reachX = Math.Abs(dx) > 1e-9 ? (halfWidth + margin) / Math.Abs(dx) : double.PositiveInfinity;
            var reachY = Math.Abs(dy) > 1e-9 ? (halfHeight + margin) / Math.Abs(dy) : double.PositiveInfinity;
            var distance = Math.Min(reachX, reachY);
            x = (float)(dx * distance);
            y = (float)(dy * distance);
        }
    }
}

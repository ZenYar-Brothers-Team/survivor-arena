using System;
using Game.Content;
using UnityEngine;

namespace Game.ActiveSkill
{
    /// <summary>World-space rectangle of the visible gameplay screen. Player attacks choose
    /// enemies and points only inside it (Game Design «Управление, бой и выживание», DECISION-0058).</summary>
    public readonly struct TargetViewportRect
    {
        /// <summary>Rejection-sampling budget for a point inside both the screen and a search circle.</summary>
        private const int PointAttempts = 16;

        public Vector2 Center { get; }
        public float HalfWidth { get; }
        public float HalfHeight { get; }

        public TargetViewportRect(Vector2 center, float halfWidth, float halfHeight)
        {
            NumericValidation.ValidatePositive(halfWidth, nameof(halfWidth));
            NumericValidation.ValidatePositive(halfHeight, nameof(halfHeight));
            Center = center;
            HalfWidth = halfWidth;
            HalfHeight = halfHeight;
        }

        public bool Contains(Vector2 point) =>
            Mathf.Abs(point.x - Center.x) <= HalfWidth && Mathf.Abs(point.y - Center.y) <= HalfHeight;

        /// <summary>Uniform point on screen and, when <paramref name="radius"/> &gt; 0, within that
        /// distance of <paramref name="origin"/>. False when the sampled budget finds none.</summary>
        public bool TryPickPoint(Vector2 origin, float radius, System.Random random, out Vector2 point)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            NumericValidation.ValidateNonNegative(radius, nameof(radius));
            for (var attempt = 0; attempt < PointAttempts; attempt++)
            {
                point = new Vector2(
                    Center.x + (float)(random.NextDouble() * 2d - 1d) * HalfWidth,
                    Center.y + (float)(random.NextDouble() * 2d - 1d) * HalfHeight);
                if (radius <= 0f || (point - origin).sqrMagnitude <= radius * radius) return true;
            }
            point = default;
            return false;
        }
    }
}

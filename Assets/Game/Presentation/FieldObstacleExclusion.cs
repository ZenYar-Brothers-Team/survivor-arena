using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>A reserved circle that subsequently generated obstacle rectangles must leave clear.</summary>
    public readonly struct FieldObstacleExclusion
    {
        public Vector2 Center { get; }
        public float Radius { get; }

        public FieldObstacleExclusion(Vector2 center, float radius)
        {
            NumericValidation.ValidateFinite(center.x, nameof(center));
            NumericValidation.ValidateFinite(center.y, nameof(center));
            NumericValidation.ValidatePositive(radius, nameof(radius));
            Center = center; Radius = radius;
        }
    }
}

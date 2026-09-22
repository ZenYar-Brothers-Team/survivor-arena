using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    // DECISION-0039: authoring-time silhouette fit, fixed at runtime.
    public sealed class SpriteContactProfile
    {
        public float Radius { get; }
        public float CenterY { get; }
        public SpriteContactProfile(float radius, float centerY)
        {
            NumericValidation.ValidatePositive(radius, nameof(radius));
            NumericValidation.ValidateNonNegativeFinite(centerY, nameof(centerY));
            Radius = radius;
            CenterY = centerY;
        }

        public void Apply(CircleCollider2D collider, float rootScale = 1)
        {
            NumericValidation.ValidatePositive(rootScale, nameof(rootScale));
            collider.radius = Radius / rootScale;
            collider.offset = Vector2.zero;
        }
    }
}

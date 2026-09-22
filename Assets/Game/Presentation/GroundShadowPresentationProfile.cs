using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Shared tuning for the lightweight procedural shadow beneath character bodies.</summary>
    public sealed class GroundShadowPresentationProfile
    {
        public float FallbackWidth { get; }
        public float ContactWidthScale { get; }
        public float Height { get; }
        public float OffsetY { get; }
        public float FallbackCenterY { get; }
        public Color Color { get; }

        public GroundShadowPresentationProfile(float fallbackWidth, float contactWidthScale,
            float height, float offsetY, float fallbackCenterY, Color color)
        {
            NumericValidation.ValidatePositive(fallbackWidth, nameof(fallbackWidth));
            NumericValidation.ValidatePositive(contactWidthScale, nameof(contactWidthScale));
            NumericValidation.ValidatePositive(height, nameof(height));
            NumericValidation.ValidateFinite(offsetY, nameof(offsetY));
            NumericValidation.ValidateNonNegativeFinite(fallbackCenterY, nameof(fallbackCenterY));
            NumericValidation.ValidateRange(color.r, 0f, 1f, nameof(color));
            NumericValidation.ValidateRange(color.g, 0f, 1f, nameof(color));
            NumericValidation.ValidateRange(color.b, 0f, 1f, nameof(color));
            NumericValidation.ValidateRange(color.a, 0f, 1f, nameof(color));
            FallbackWidth = fallbackWidth;
            ContactWidthScale = contactWidthScale;
            Height = height;
            OffsetY = offsetY;
            FallbackCenterY = fallbackCenterY;
            Color = color;
        }
    }
}

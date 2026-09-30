using System;
using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Tuning of the four slow-status candidate looks (DECISION-0108). World units are relative to the enemy
    /// VisualRoot, which is scale-compensated to world size. Colors are RGBA 0..1.
    /// </summary>
    public sealed class SlowStatusPresentationProfile
    {
        /// <summary>Multiplied into the body color while slowed (Tint).</summary>
        public Color TintColor { get; }
        /// <summary>Tint and opacity of the shared ice texture clipped to the body (Ice).</summary>
        public Color IceColor { get; }
        /// <summary>Shared approved mask resolved through the presentation catalog.</summary>
        public SpriteDefinition IceMask { get; }
        /// <summary>Solid silhouette copies drawn behind the body (Outline).</summary>
        public Color OutlineColor { get; }
        /// <summary>Outline thickness, world units (0.01–0.1).</summary>
        public float OutlineWidth { get; }
        /// <summary>Full bar width / height in world units; gap from visible sprite bottom to bar top.</summary>
        public float BarWidth { get; }
        public float BarHeight { get; }
        public float BarOffsetY { get; }
        public Color BarFillColor { get; }
        public Color BarBackColor { get; }
        /// <summary>Development-only «slow all enemies» command: movement reduction 0..1 and seconds.</summary>
        public float PreviewSlowFraction { get; }
        public float PreviewSlowSeconds { get; }

        public SlowStatusPresentationProfile(Color tintColor, Color iceColor, Color outlineColor, float outlineWidth,
            float barWidth, float barHeight, float barOffsetY, Color barFillColor, Color barBackColor,
            float previewSlowFraction, float previewSlowSeconds, SpriteDefinition iceMask)
        {
            Validate(tintColor, nameof(tintColor));
            Validate(iceColor, nameof(iceColor));
            Validate(outlineColor, nameof(outlineColor));
            Validate(barFillColor, nameof(barFillColor));
            Validate(barBackColor, nameof(barBackColor));
            IceMask = iceMask ?? throw new ArgumentNullException(nameof(iceMask));
            IceMask.RequireRole(SpriteRole.Mask);
            NumericValidation.ValidateRange(outlineWidth, 0.005f, 0.2f, nameof(outlineWidth));
            NumericValidation.ValidatePositive(barWidth, nameof(barWidth));
            NumericValidation.ValidatePositive(barHeight, nameof(barHeight));
            NumericValidation.ValidateNonNegativeFinite(barOffsetY, nameof(barOffsetY));
            NumericValidation.ValidateRange(previewSlowFraction, 0.01f, 0.95f, nameof(previewSlowFraction));
            NumericValidation.ValidatePositive(previewSlowSeconds, nameof(previewSlowSeconds));
            TintColor = tintColor;
            IceColor = iceColor;
            OutlineColor = outlineColor;
            OutlineWidth = outlineWidth;
            BarWidth = barWidth;
            BarHeight = barHeight;
            BarOffsetY = barOffsetY;
            BarFillColor = barFillColor;
            BarBackColor = barBackColor;
            PreviewSlowFraction = previewSlowFraction;
            PreviewSlowSeconds = previewSlowSeconds;
        }

        private static void Validate(Color color, string name)
        {
            NumericValidation.ValidateRange(color.r, 0f, 1f, name);
            NumericValidation.ValidateRange(color.g, 0f, 1f, name);
            NumericValidation.ValidateRange(color.b, 0f, 1f, name);
            NumericValidation.ValidateRange(color.a, 0f, 1f, name);
        }
    }
}

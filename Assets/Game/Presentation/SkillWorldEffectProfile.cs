using System;
using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Presentation-only colors/timings for procedural skill world effects (Art Production:
    /// "Procedural in Unity / Hybrid"). Never owns hit geometry: sizes come from gameplay radii.
    /// </summary>
    public sealed class SkillWorldEffectProfile
    {
        public ContentId SkillId { get; }
        public SkillWorldEffectKind Kind { get; }
        public Color Color { get; }
        public Color ImpactColor { get; }
        /// <summary>Ring/arc line width in world units.</summary>
        public float Thickness { get; }
        /// <summary>Fade time of a finished ring, arc or impact flash, in running seconds.</summary>
        public float FadeSeconds { get; }

        public SkillWorldEffectProfile(ContentId skillId, SkillWorldEffectKind kind, Color color, Color impactColor,
            float thickness, float fadeSeconds)
        {
            if (!skillId.IsValid) throw new ArgumentException("Skill world effect requires a skill id.", nameof(skillId));
            if (!Enum.IsDefined(typeof(SkillWorldEffectKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            NumericValidation.ValidatePositive(thickness, nameof(thickness));
            NumericValidation.ValidatePositive(fadeSeconds, nameof(fadeSeconds));
            ValidateColor(color, nameof(color));
            ValidateColor(impactColor, nameof(impactColor));
            SkillId = skillId;
            Kind = kind;
            Color = color;
            ImpactColor = impactColor;
            Thickness = thickness;
            FadeSeconds = fadeSeconds;
        }

        private static void ValidateColor(Color color, string name)
        {
            NumericValidation.ValidateRange(color.r, 0f, 1f, name);
            NumericValidation.ValidateRange(color.g, 0f, 1f, name);
            NumericValidation.ValidateRange(color.b, 0f, 1f, name);
            NumericValidation.ValidateRange(color.a, 0f, 1f, name);
        }
    }
}

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
        /// <summary>Width of the vertical light pillar over a strike impact, in world units (0 = no pillar).</summary>
        public float PillarWidth { get; }
        /// <summary>Height of that pillar above the impact point, in world units (0 = no pillar).</summary>
        public float PillarHeight { get; }
        public bool HasPillar => PillarHeight > 0f;
        /// <summary>Seconds before the impact at which the pillar appears, so light lands before the
        /// flash (0 = together with it; DECISION-0058). Clamped to the strike telegraph at runtime.</summary>
        public float PillarLeadSeconds { get; }

        public SkillWorldEffectProfile(ContentId skillId, SkillWorldEffectKind kind, Color color, Color impactColor,
            float thickness, float fadeSeconds, float pillarWidth = 0f, float pillarHeight = 0f,
            float pillarLeadSeconds = 0f)
        {
            if (!skillId.IsValid) throw new ArgumentException("Skill world effect requires a skill id.", nameof(skillId));
            if (!Enum.IsDefined(typeof(SkillWorldEffectKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            NumericValidation.ValidatePositive(thickness, nameof(thickness));
            NumericValidation.ValidatePositive(fadeSeconds, nameof(fadeSeconds));
            NumericValidation.ValidateNonNegative(pillarWidth, nameof(pillarWidth));
            NumericValidation.ValidateNonNegative(pillarHeight, nameof(pillarHeight));
            if ((pillarWidth > 0f) != (pillarHeight > 0f))
                throw new ArgumentException("A strike pillar needs both width and height.", nameof(pillarHeight));
            if (pillarHeight > 0f && kind != SkillWorldEffectKind.StrikeTelegraph)
                throw new ArgumentException("Only strike effects can have a light pillar.", nameof(pillarHeight));
            NumericValidation.ValidateNonNegativeFinite(pillarLeadSeconds, nameof(pillarLeadSeconds));
            if (pillarLeadSeconds > 0f && pillarHeight <= 0f)
                throw new ArgumentException("Pillar lead requires a pillar.", nameof(pillarLeadSeconds));
            ValidateColor(color, nameof(color));
            ValidateColor(impactColor, nameof(impactColor));
            SkillId = skillId;
            Kind = kind;
            Color = color;
            ImpactColor = impactColor;
            Thickness = thickness;
            FadeSeconds = fadeSeconds;
            PillarWidth = pillarWidth;
            PillarHeight = pillarHeight;
            PillarLeadSeconds = pillarLeadSeconds;
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

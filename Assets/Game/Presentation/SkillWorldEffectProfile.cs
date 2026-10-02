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
        /// <summary>Ring/arc line width in world units; for a beam, the width of its bright core.</summary>
        public float Thickness { get; }
        /// <summary>Fade time in running seconds; for a travelling cone, total lifetime including ExpansionSeconds.</summary>
        public float FadeSeconds { get; }
        /// <summary>ConeArc travel from caster to full radius in running seconds; remaining lifetime fades it.</summary>
        public float ExpansionSeconds { get; }
        /// <summary>Pressure-wave band width / radius; zero retains the plain fixture shape.</summary>
        public float BandFraction { get; }
        /// <summary>Contour displacement / band width, 0..0.25.</summary>
        public float ContourVariation { get; }
        public int AccentCount { get; }
        /// <summary>Short radial strokes, measured as fractions of radius and radians respectively.</summary>
        public float AccentLengthFraction { get; }
        public float AccentWidthRadians { get; }
        /// <summary>Tail alpha uses this power during fade; zero disables separate tail presentation.</summary>
        public float TailFadePower { get; }
        /// <summary>Width of the vertical light pillar over a strike impact, in world units (0 = no pillar).</summary>
        public float PillarWidth { get; }
        /// <summary>Height of that pillar above the impact point, in world units (0 = no pillar).</summary>
        public float PillarHeight { get; }
        public bool HasPillar => PillarHeight > 0f;
        /// <summary>Seconds before the impact at which the pillar appears, so light lands before the
        /// flash (0 = together with it; DECISION-0058). Clamped to the strike telegraph at runtime.</summary>
        public float PillarLeadSeconds { get; }
        /// <summary>Beam only: period in running seconds of the breathing width and the travelling bright wave (0 = steady beam).</summary>
        public float PulseSeconds { get; }
        /// <summary>Beam only: width breathing amplitude as a fraction of the width, 0..0.5.</summary>
        public float PulseDepth { get; }

        public SkillWorldEffectProfile(ContentId skillId, SkillWorldEffectKind kind, Color color, Color impactColor,
            float thickness, float fadeSeconds, float pillarWidth = 0f, float pillarHeight = 0f,
            float pillarLeadSeconds = 0f, float expansionSeconds = 0f,
            float bandFraction = 0f, float contourVariation = 0f, int accentCount = 0,
            float accentLengthFraction = 0f, float accentWidthRadians = 0f, float tailFadePower = 0f,
            float pulseSeconds = 0f, float pulseDepth = 0f)
        {
            if (!skillId.IsValid) throw new ArgumentException("Skill world effect requires a skill id.", nameof(skillId));
            if (!Enum.IsDefined(typeof(SkillWorldEffectKind), kind)) throw new ArgumentOutOfRangeException(nameof(kind));
            NumericValidation.ValidatePositive(thickness, nameof(thickness));
            NumericValidation.ValidatePositive(fadeSeconds, nameof(fadeSeconds));
            NumericValidation.ValidateNonNegativeFinite(expansionSeconds, nameof(expansionSeconds));
            if (expansionSeconds >= fadeSeconds || (expansionSeconds > 0f && kind != SkillWorldEffectKind.ConeArc))
                throw new ArgumentException("Only a cone can travel, and travel must leave time to fade.", nameof(expansionSeconds));
            NumericValidation.ValidateNonNegative(pillarWidth, nameof(pillarWidth));
            NumericValidation.ValidateNonNegative(pillarHeight, nameof(pillarHeight));
            if ((pillarWidth > 0f) != (pillarHeight > 0f))
                throw new ArgumentException("A strike pillar needs both width and height.", nameof(pillarHeight));
            if (pillarHeight > 0f && kind != SkillWorldEffectKind.StrikeTelegraph)
                throw new ArgumentException("Only strike effects can have a light pillar.", nameof(pillarHeight));
            NumericValidation.ValidateNonNegativeFinite(pillarLeadSeconds, nameof(pillarLeadSeconds));
            if (pillarLeadSeconds > 0f && pillarHeight <= 0f)
                throw new ArgumentException("Pillar lead requires a pillar.", nameof(pillarLeadSeconds));
            NumericValidation.ValidateNonNegativeFinite(pulseSeconds, nameof(pulseSeconds));
            NumericValidation.ValidateRange(pulseDepth, 0f, .5f, nameof(pulseDepth));
            if ((pulseSeconds > 0f) != (pulseDepth > 0f))
                throw new ArgumentException("A beam pulse needs both period and depth.", nameof(pulseDepth));
            if (pulseSeconds > 0f && kind != SkillWorldEffectKind.Beam)
                throw new ArgumentException("Only beams can pulse.", nameof(pulseSeconds));
            ValidateColor(color, nameof(color));
            ValidateColor(impactColor, nameof(impactColor));
            SkillId = skillId;
            Kind = kind;
            Color = color;
            ImpactColor = impactColor;
            Thickness = thickness;
            FadeSeconds = fadeSeconds;
            ExpansionSeconds = expansionSeconds;
            NumericValidation.ValidateRange(bandFraction, 0f, .5f, nameof(bandFraction));
            NumericValidation.ValidateRange(contourVariation, 0f, .25f, nameof(contourVariation));
            NumericValidation.ValidateRange(accentCount, 0, 12, nameof(accentCount));
            NumericValidation.ValidateRange(accentLengthFraction, 0f, .5f, nameof(accentLengthFraction));
            NumericValidation.ValidateRange(accentWidthRadians, 0f, .1f, nameof(accentWidthRadians));
            NumericValidation.ValidateRange(tailFadePower, 0f, 8f, nameof(tailFadePower));
            if (bandFraction > 0f)
            {
                if (kind != SkillWorldEffectKind.ExpandingRing && kind != SkillWorldEffectKind.ConeArc)
                    throw new ArgumentException("Pressure bands require a ring or cone.", nameof(bandFraction));
                if (tailFadePower < 1f || (accentCount > 0 && (accentLengthFraction <= 0f || accentWidthRadians <= 0f)))
                    throw new ArgumentException("Pressure bands require tail fade and complete accent dimensions.");
            }
            else if (contourVariation != 0f || accentCount != 0 || accentLengthFraction != 0f || accentWidthRadians != 0f || tailFadePower != 0f)
                throw new ArgumentException("Pressure detail requires a nonzero band fraction.");
            BandFraction = bandFraction;
            ContourVariation = contourVariation;
            AccentCount = accentCount;
            AccentLengthFraction = accentLengthFraction;
            AccentWidthRadians = accentWidthRadians;
            TailFadePower = tailFadePower;
            PillarWidth = pillarWidth;
            PillarHeight = pillarHeight;
            PillarLeadSeconds = pillarLeadSeconds;
            PulseSeconds = pulseSeconds;
            PulseDepth = pulseDepth;
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

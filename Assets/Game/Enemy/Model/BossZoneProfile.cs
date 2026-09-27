using System;
using Game.Combat;
using Game.Content;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// Boss ground zone (DECISION-0066, family F1). A circle outline appears and fills from the center over
    /// <see cref="FillSeconds"/>; when full it hits the player inside (SafeCircles: outside every circle) once with
    /// <see cref="Damage"/> and <see cref="Controls"/>. Optional burning ground then deals
    /// <see cref="LingerDamagePerSecond"/> × <see cref="LingerTickSeconds"/> every tick for <see cref="LingerSeconds"/>.
    /// Distances are world units, times are running seconds.
    /// </summary>
    public sealed class BossZoneProfile
    {
        public BossZonePlacement Placement { get; }
        public int Count { get; }
        public float Radius { get; }
        /// <summary>AroundPlayer/SafeCircles: circle centers lie within this distance of the player.</summary>
        public float ScatterRadius { get; }
        /// <summary>AroundPlayer/SafeCircles: minimum distance between circle centers.</summary>
        public float MinSpacing { get; }
        /// <summary>Trail: time between consecutive circles.</summary>
        public float IntervalSeconds { get; }
        public float FillSeconds { get; }
        public float Damage { get; }
        public CombatControlProfile Controls { get; }
        public float LingerSeconds { get; }
        public float LingerDamagePerSecond { get; }
        public float LingerTickSeconds { get; }
        /// <summary>Length of the visual flash after the hit.</summary>
        public float ImpactEffectSeconds { get; }
        public Color TelegraphColor { get; }
        public Color ImpactColor { get; }

        public BossZoneProfile(BossZonePlacement placement, int count, float radius, float scatterRadius, float minSpacing,
            float intervalSeconds, float fillSeconds, float damage, CombatControlProfile controls, float lingerSeconds,
            float lingerDamagePerSecond, float lingerTickSeconds, float impactEffectSeconds, Color telegraphColor, Color impactColor)
        {
            if (!Enum.IsDefined(typeof(BossZonePlacement), placement)) throw new ArgumentOutOfRangeException(nameof(placement));
            NumericValidation.ValidateCount(count, nameof(count));
            NumericValidation.ValidatePositive(radius, nameof(radius));
            NumericValidation.ValidateNonNegativeFinite(scatterRadius, nameof(scatterRadius));
            NumericValidation.ValidateNonNegativeFinite(minSpacing, nameof(minSpacing));
            NumericValidation.ValidateNonNegativeFinite(intervalSeconds, nameof(intervalSeconds));
            NumericValidation.ValidatePositive(fillSeconds, nameof(fillSeconds));
            NumericValidation.ValidateNonNegativeFinite(damage, nameof(damage));
            NumericValidation.ValidateNonNegativeFinite(lingerSeconds, nameof(lingerSeconds));
            NumericValidation.ValidateNonNegativeFinite(lingerDamagePerSecond, nameof(lingerDamagePerSecond));
            NumericValidation.ValidateNonNegativeFinite(lingerTickSeconds, nameof(lingerTickSeconds));
            NumericValidation.ValidatePositive(impactEffectSeconds, nameof(impactEffectSeconds));
            var single = placement == BossZonePlacement.AtPlayer || placement == BossZonePlacement.AroundSelf;
            if (single && count != 1) throw new ArgumentException($"{placement} is a single circle.", nameof(count));
            var scattered = placement == BossZonePlacement.AroundPlayer || placement == BossZonePlacement.SafeCircles;
            if (scattered && (scatterRadius <= 0f || minSpacing <= 0f))
                throw new ArgumentException($"{placement} needs a scatter radius and spacing.", nameof(scatterRadius));
            if (placement == BossZonePlacement.Trail && (count < 2 || intervalSeconds <= 0f))
                throw new ArgumentException("A trail needs several circles and an interval.", nameof(intervalSeconds));
            if (lingerSeconds > 0f && (lingerDamagePerSecond <= 0f || lingerTickSeconds <= 0f))
                throw new ArgumentException("Burning ground needs damage and a tick.", nameof(lingerDamagePerSecond));
            if (placement == BossZonePlacement.SafeCircles && lingerSeconds > 0f)
                throw new ArgumentException("Safe circles do not leave burning ground.", nameof(lingerSeconds));
            Placement = placement;
            Count = count;
            Radius = radius;
            ScatterRadius = scatterRadius;
            MinSpacing = minSpacing;
            IntervalSeconds = intervalSeconds;
            FillSeconds = fillSeconds;
            Damage = damage;
            Controls = controls ?? throw new ArgumentNullException(nameof(controls));
            LingerSeconds = lingerSeconds;
            LingerDamagePerSecond = lingerDamagePerSecond;
            LingerTickSeconds = lingerTickSeconds;
            ImpactEffectSeconds = impactEffectSeconds;
            TelegraphColor = telegraphColor;
            ImpactColor = impactColor;
        }
    }
}

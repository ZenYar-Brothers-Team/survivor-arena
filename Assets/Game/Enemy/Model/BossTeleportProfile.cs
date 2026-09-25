using System;
using Game.Combat;
using Game.Content;
using Game.Enemy.Json;
using UnityEngine;

namespace Game.Enemy
{
    /// <summary>
    /// Boss teleport-slam (BOSS-001, DECISION-0059): after the player stays farther than <see cref="FarDistance"/>
    /// for <see cref="FarSeconds"/> of run time, a ground marker appears <see cref="LandingDistance"/> from the
    /// player in a uniformly random direction; after <see cref="TelegraphSeconds"/> the boss lands there and hits the player if
    /// they are within <see cref="ImpactRadius"/> of the landing point.
    /// </summary>
    public sealed class BossTeleportProfile
    {
        public float FarDistance { get; }
        public float FarSeconds { get; }
        public float LandingDistance { get; }
        public float TelegraphSeconds { get; }
        public float ImpactRadius { get; }
        public float ImpactDamage { get; }
        public CombatControlProfile ImpactControls { get; }
        public float ImpactEffectSeconds { get; }
        public Color TelegraphColor { get; }
        public Color ImpactColor { get; }

        public BossTeleportProfile(float farDistance, float farSeconds, float landingDistance, float telegraphSeconds,
            float impactRadius, float impactDamage, CombatControlProfile impactControls, float impactEffectSeconds,
            Color telegraphColor, Color impactColor)
        {
            NumericValidation.ValidatePositive(farDistance, nameof(farDistance));
            NumericValidation.ValidatePositive(farSeconds, nameof(farSeconds));
            NumericValidation.ValidatePositive(landingDistance, nameof(landingDistance));
            NumericValidation.ValidatePositive(telegraphSeconds, nameof(telegraphSeconds));
            NumericValidation.ValidatePositive(impactRadius, nameof(impactRadius));
            NumericValidation.ValidateNonNegative(impactDamage, nameof(impactDamage));
            NumericValidation.ValidatePositive(impactEffectSeconds, nameof(impactEffectSeconds));
            if (landingDistance >= farDistance)
                throw new ArgumentException("Landing must be closer to the player than the far distance.", nameof(landingDistance));
            FarDistance = farDistance;
            FarSeconds = farSeconds;
            LandingDistance = landingDistance;
            TelegraphSeconds = telegraphSeconds;
            ImpactRadius = impactRadius;
            ImpactDamage = impactDamage;
            ImpactControls = impactControls ?? throw new ArgumentNullException(nameof(impactControls));
            ImpactEffectSeconds = impactEffectSeconds;
            TelegraphColor = telegraphColor;
            ImpactColor = impactColor;
        }

        /// <summary>Null data means no teleport; every present field is required.</summary>
        public static BossTeleportProfile FromData(BossTeleportData data, string owner)
        {
            if (data == null) return null;
            if (data.ImpactControls?.KnockbackDistance == null)
                throw new InvalidOperationException($"{owner}.teleport.impactControls.knockbackDistance must be explicit, including zero.");
            return new BossTeleportProfile(
                Require(data.FarDistance, owner, "farDistance"), Require(data.FarSeconds, owner, "farSeconds"),
                Require(data.LandingDistance, owner, "landingDistance"), Require(data.TelegraphSeconds, owner, "telegraphSeconds"),
                Require(data.ImpactRadius, owner, "impactRadius"), Require(data.ImpactDamage, owner, "impactDamage"),
                data.ImpactControls.ToProfile(), Require(data.ImpactEffectSeconds, owner, "impactEffectSeconds"),
                RequireColor(data.TelegraphColor, owner, "telegraphColor"), RequireColor(data.ImpactColor, owner, "impactColor"));
        }

        private static float Require(float? value, string owner, string field) =>
            value ?? throw new InvalidOperationException($"{owner}.teleport.{field} must be set in config.");

        private static Color RequireColor(float[] value, string owner, string field)
        {
            if (value == null || value.Length != 4)
                throw new InvalidOperationException($"{owner}.teleport.{field} requires RGBA.");
            foreach (var channel in value) NumericValidation.ValidateRange(channel, 0f, 1f, field);
            return new Color(value[0], value[1], value[2], value[3]);
        }
    }
}

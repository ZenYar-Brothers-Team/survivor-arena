using System;
using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Immutable visual motion and impact settings attached to a projectile sprite.</summary>
    public sealed class ProjectilePresentationProfile
    {
        public float VisualScale { get; }
        public float SpinDegreesPerSecond { get; }
        public float ImpactDurationSeconds { get; }
        public float FlashSize { get; }
        public Color FlashColor { get; }
        public int ParticleCount { get; }
        public float ParticleSize { get; }
        public float ParticleSpeed { get; }
        public Color ParticleColor { get; }
        public ExplosionPresentationProfile Explosion { get; }
        /// <summary>Hostile-projectile halo; null for player projectiles.</summary>
        public ProjectileThreatHaloProfile ThreatHalo { get; }

        /// <summary>Creates validated visual scale, spin and material-impact settings.</summary>
        public ProjectilePresentationProfile(float visualScale, float spinDegreesPerSecond,
            float impactDurationSeconds, float flashSize, Color flashColor, int particleCount,
            float particleSize, float particleSpeed, Color particleColor,
            ExplosionPresentationProfile explosion = null, ProjectileThreatHaloProfile threatHalo = null)
        {
            NumericValidation.ValidatePositive(visualScale, nameof(visualScale));
            NumericValidation.ValidateFinite(spinDegreesPerSecond, nameof(spinDegreesPerSecond));
            NumericValidation.ValidatePositive(impactDurationSeconds, nameof(impactDurationSeconds));
            NumericValidation.ValidatePositive(flashSize, nameof(flashSize));
            NumericValidation.ValidateRange(particleCount, 2, 4, nameof(particleCount));
            NumericValidation.ValidatePositive(particleSize, nameof(particleSize));
            NumericValidation.ValidatePositive(particleSpeed, nameof(particleSpeed));
            ValidateColor(flashColor, nameof(flashColor));
            ValidateColor(particleColor, nameof(particleColor));
            VisualScale = visualScale;
            SpinDegreesPerSecond = spinDegreesPerSecond;
            ImpactDurationSeconds = impactDurationSeconds;
            FlashSize = flashSize;
            FlashColor = flashColor;
            ParticleCount = particleCount;
            ParticleSize = particleSize;
            ParticleSpeed = particleSpeed;
            ParticleColor = particleColor;
            Explosion = explosion;
            ThreatHalo = threatHalo;
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

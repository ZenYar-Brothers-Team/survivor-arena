using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>Validated parameters for a reusable, particle-only world explosion burst.</summary>
    public sealed class ExplosionPresentationProfile
    {
        public float DurationSeconds { get; }
        public float FlashSizeMultiplier { get; }
        public Color FlashColor { get; }
        public int ParticleCount { get; }
        public float ParticleSizeMultiplier { get; }
        public float ParticleSpeedMultiplier { get; }
        public Color ParticleColor { get; }

        /// <summary>Creates an explosion profile whose sizes and speed scale from the gameplay blast radius.</summary>
        public ExplosionPresentationProfile(float durationSeconds, float flashSizeMultiplier, Color flashColor,
            int particleCount, float particleSizeMultiplier, float particleSpeedMultiplier, Color particleColor)
        {
            NumericValidation.ValidatePositive(durationSeconds, nameof(durationSeconds));
            NumericValidation.ValidatePositive(flashSizeMultiplier, nameof(flashSizeMultiplier));
            NumericValidation.ValidateRange(particleCount, 3, 12, nameof(particleCount));
            NumericValidation.ValidatePositive(particleSizeMultiplier, nameof(particleSizeMultiplier));
            NumericValidation.ValidatePositive(particleSpeedMultiplier, nameof(particleSpeedMultiplier));
            ValidateColor(flashColor, nameof(flashColor));
            ValidateColor(particleColor, nameof(particleColor));
            DurationSeconds = durationSeconds;
            FlashSizeMultiplier = flashSizeMultiplier;
            FlashColor = flashColor;
            ParticleCount = particleCount;
            ParticleSizeMultiplier = particleSizeMultiplier;
            ParticleSpeedMultiplier = particleSpeedMultiplier;
            ParticleColor = particleColor;
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

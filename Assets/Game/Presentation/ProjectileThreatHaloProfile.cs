using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    /// <summary>
    /// Pulsing coral-red disc drawn under hostile projectiles so the player sees what to dodge
    /// (Art Direction: hostile danger uses a coral/red core and differs from friendly attacks by color
    /// and form/pulse; playtest 2026-09-24_e1e04fc4 OBS-06). Presentation only; the collider is unchanged.
    /// </summary>
    public sealed class ProjectileThreatHaloProfile
    {
        /// <summary>Halo diameter as a multiple of the projectile's collision diameter.</summary>
        public float Scale { get; }
        public Color Color { get; }
        /// <summary>Full pulse period in running seconds.</summary>
        public float PulseSeconds { get; }
        /// <summary>Relative size change at the pulse peak (0 = steady).</summary>
        public float PulseAmplitude { get; }

        public ProjectileThreatHaloProfile(float scale, Color color, float pulseSeconds, float pulseAmplitude)
        {
            NumericValidation.ValidatePositive(scale, nameof(scale));
            NumericValidation.ValidatePositive(pulseSeconds, nameof(pulseSeconds));
            NumericValidation.ValidateRange(pulseAmplitude, 0f, 1f, nameof(pulseAmplitude));
            NumericValidation.ValidateRange(color.r, 0f, 1f, nameof(color));
            NumericValidation.ValidateRange(color.g, 0f, 1f, nameof(color));
            NumericValidation.ValidateRange(color.b, 0f, 1f, nameof(color));
            NumericValidation.ValidateRange(color.a, 0f, 1f, nameof(color));
            Scale = scale;
            Color = color;
            PulseSeconds = pulseSeconds;
            PulseAmplitude = pulseAmplitude;
        }

        /// <summary>Size multiplier at <paramref name="elapsedSeconds"/> of running time.</summary>
        public float PulseFactor(float elapsedSeconds) =>
            1f + PulseAmplitude * Mathf.Sin(elapsedSeconds / PulseSeconds * Mathf.PI * 2f);
    }
}

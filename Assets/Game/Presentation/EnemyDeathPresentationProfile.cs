using Game.Content;
using UnityEngine;

namespace Game.Presentation
{
    public sealed class EnemyDeathPresentationProfile
    {
        public float SquashDurationSeconds { get; }
        public float FadeDurationSeconds { get; }
        public float SquashWidthScale { get; }
        public float SquashHeightScale { get; }
        public float EndScale { get; }
        public Color EndColor { get; }
        public int DustCount { get; }
        public float DustLifetimeSeconds { get; }
        public float DustSpeed { get; }
        public float DustSize { get; }
        public Color DustColor { get; }
        public float TotalDurationSeconds => SquashDurationSeconds + FadeDurationSeconds;

        public EnemyDeathPresentationProfile(float squashDurationSeconds, float fadeDurationSeconds,
            float squashWidthScale, float squashHeightScale, float endScale, Color endColor,
            int dustCount, float dustLifetimeSeconds, float dustSpeed, float dustSize, Color dustColor)
        {
            NumericValidation.ValidatePositive(squashDurationSeconds, nameof(squashDurationSeconds));
            NumericValidation.ValidatePositive(fadeDurationSeconds, nameof(fadeDurationSeconds));
            NumericValidation.ValidateRange(squashWidthScale, 1f, 2f, nameof(squashWidthScale));
            NumericValidation.ValidateRange(squashHeightScale, 0.1f, 1f, nameof(squashHeightScale));
            NumericValidation.ValidateRange(endScale, 0.01f, 1f, nameof(endScale));
            NumericValidation.ValidateRange(endColor.r, 0f, 1f, nameof(endColor));
            NumericValidation.ValidateRange(endColor.g, 0f, 1f, nameof(endColor));
            NumericValidation.ValidateRange(endColor.b, 0f, 1f, nameof(endColor));
            NumericValidation.ValidateRange(endColor.a, 0f, 1f, nameof(endColor));
            NumericValidation.ValidateRange(dustCount, 1, 12, nameof(dustCount));
            NumericValidation.ValidatePositive(dustLifetimeSeconds, nameof(dustLifetimeSeconds));
            NumericValidation.ValidatePositive(dustSpeed, nameof(dustSpeed));
            NumericValidation.ValidatePositive(dustSize, nameof(dustSize));
            NumericValidation.ValidateRange(dustColor.r, 0f, 1f, nameof(dustColor));
            NumericValidation.ValidateRange(dustColor.g, 0f, 1f, nameof(dustColor));
            NumericValidation.ValidateRange(dustColor.b, 0f, 1f, nameof(dustColor));
            NumericValidation.ValidateRange(dustColor.a, 0f, 1f, nameof(dustColor));
            SquashDurationSeconds = squashDurationSeconds;
            FadeDurationSeconds = fadeDurationSeconds;
            SquashWidthScale = squashWidthScale;
            SquashHeightScale = squashHeightScale;
            EndScale = endScale;
            EndColor = endColor;
            DustCount = dustCount;
            DustLifetimeSeconds = dustLifetimeSeconds;
            DustSpeed = dustSpeed;
            DustSize = dustSize;
            DustColor = dustColor;
        }
    }
}

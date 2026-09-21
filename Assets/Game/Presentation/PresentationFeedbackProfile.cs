using Game.Content;

namespace Game.Presentation
{
    public sealed class PresentationFeedbackProfile
    {
        public float FadeSeconds { get; }
        public float ProcSeconds { get; }
        public float ProcScale { get; }

        public PresentationFeedbackProfile(float fadeSeconds, float procSeconds, float procScale)
        {
            NumericValidation.ValidatePositive(fadeSeconds, nameof(fadeSeconds));
            NumericValidation.ValidatePositive(procSeconds, nameof(procSeconds));
            NumericValidation.ValidateRange(procScale, 0, .1f, nameof(procScale));
            FadeSeconds = fadeSeconds;
            ProcSeconds = procSeconds;
            ProcScale = procScale;
        }
    }
}

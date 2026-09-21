using System;
using Game.Content;

namespace Game.Presentation
{
    public sealed class ScreenShakeRequestGate
    {
        private readonly IScreenShakePreference _preference;
        public event Action<float, float> Requested;
        public ScreenShakeRequestGate(IScreenShakePreference preference)
        {
            _preference = preference ?? throw new ArgumentNullException(nameof(preference));
        }
        public void Request(bool isRunning, float amplitude, float seconds)
        {
            NumericValidation.ValidateNonNegativeFinite(amplitude, nameof(amplitude));
            NumericValidation.ValidatePositive(seconds, nameof(seconds));
            if (isRunning && _preference.ScreenShakeEnabled && amplitude > 0)
                Requested?.Invoke(amplitude, seconds);
        }
    }
}

using Game.Content;

namespace Game.Automation
{
    /// <summary>AB-14: called only for running physics steps; timestamps describe the state before the step.</summary>
    public sealed class DemonstrationClock
    {
        private readonly double _interval;
        private double _elapsed;
        private double _nextSample;
        public long StepIndex { get; private set; } = -1;
        public double StepStartSeconds { get; private set; }

        public DemonstrationClock(float interval)
        {
            NumericValidation.ValidateRange(interval, 0.02f, 0.5f, nameof(interval));
            _interval = interval;
        }

        public bool Advance(float deltaTime)
        {
            NumericValidation.ValidatePositive(deltaTime, nameof(deltaTime));
            StepIndex++;
            StepStartSeconds = _elapsed;
            _elapsed += deltaTime;
            if (StepStartSeconds + 0.000001 < _nextSample) return false;
            _nextSample = StepStartSeconds + _interval;
            return true;
        }
    }
}

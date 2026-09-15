using System;

namespace Game.Enemy
{
    public sealed class ContinuousContactTimer
    {
        private readonly float _intervalSeconds;
        private float _elapsedSeconds;
        private bool _isInContact;
        private bool _initialHitApplied;

        public ContinuousContactTimer(float intervalSeconds)
        {
            if (float.IsNaN(intervalSeconds) || float.IsInfinity(intervalSeconds) || intervalSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(intervalSeconds), "Contact interval must be finite and greater than zero.");

            _intervalSeconds = intervalSeconds;
        }

        public int BeginContact(bool isRunning)
        {
            _isInContact = true;
            _initialHitApplied = false;
            _elapsedSeconds = 0f;
            return ApplyInitialHitIfNeeded(isRunning);
        }

        public int Tick(float deltaTime, bool isRunning)
        {
            if (float.IsNaN(deltaTime) || float.IsInfinity(deltaTime) || deltaTime < 0f)
                throw new ArgumentOutOfRangeException(nameof(deltaTime), "Delta time must be finite and non-negative.");

            if (!_isInContact || !isRunning)
                return 0;

            var hitCount = ApplyInitialHitIfNeeded(isRunning);
            _elapsedSeconds += deltaTime;
            var repeatedHits = (int)Math.Floor(_elapsedSeconds / _intervalSeconds);
            _elapsedSeconds -= repeatedHits * _intervalSeconds;
            return hitCount + repeatedHits;
        }

        public void EndContact()
        {
            _isInContact = false;
            _initialHitApplied = false;
            _elapsedSeconds = 0f;
        }

        private int ApplyInitialHitIfNeeded(bool isRunning)
        {
            if (!isRunning || _initialHitApplied)
                return 0;

            _initialHitApplied = true;
            return 1;
        }
    }
}

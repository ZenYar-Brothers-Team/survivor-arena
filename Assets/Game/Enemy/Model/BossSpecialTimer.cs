using Game.Content;

namespace Game.Enemy
{
    /// <summary>
    /// Turn timing of a <see cref="BossSpecialAttack"/> step, matching the wind-up-start-to-start cadence of projectile
    /// steps: a fresh timer (first step of a sequence) waits one full cooldown; a restarted timer starts at once. The
    /// step is complete one cooldown after its start. Nothing advances while the run is not running.
    /// </summary>
    public sealed class BossSpecialTimer
    {
        private readonly float _cooldownSeconds;
        private float _remaining;

        public bool HasStarted { get; private set; }
        public float Remaining => _remaining;

        public BossSpecialTimer(float cooldownSeconds)
        {
            NumericValidation.ValidatePositive(cooldownSeconds, nameof(cooldownSeconds));
            _cooldownSeconds = cooldownSeconds;
            _remaining = cooldownSeconds;
        }

        /// <summary>Returns true on the tick the special starts.</summary>
        public bool Tick(float deltaTime, bool isRunning)
        {
            NumericValidation.ValidateNonNegative(deltaTime, nameof(deltaTime));
            if (!isRunning) return false;
            _remaining -= deltaTime;
            if (HasStarted || _remaining > 0f) return false;
            HasStarted = true;
            _remaining += _cooldownSeconds;
            return true;
        }

        public bool CycleCompletesWithin(float deltaTime) => HasStarted && _remaining <= deltaTime;

        public void RestartCycle()
        {
            HasStarted = false;
            _remaining = 0f;
        }
    }
}

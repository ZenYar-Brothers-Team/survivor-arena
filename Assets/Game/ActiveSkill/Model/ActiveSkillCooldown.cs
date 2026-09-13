using System;

namespace Game.ActiveSkill
{
    public sealed class ActiveSkillCooldown
    {
        public float RemainingSeconds { get; private set; }
        public bool IsReady => RemainingSeconds <= 0f;

        public void Tick(float deltaTime, bool isRunning)
        {
            ValidateNonNegativeFinite(deltaTime, nameof(deltaTime));
            if (!isRunning || deltaTime == 0f || IsReady)
                return;

            RemainingSeconds = Math.Max(0f, RemainingSeconds - deltaTime);
        }

        public void Consume(float baseCooldownSeconds, float cooldownMultiplier)
        {
            if (float.IsNaN(baseCooldownSeconds) || float.IsInfinity(baseCooldownSeconds) || baseCooldownSeconds <= 0f)
                throw new ArgumentOutOfRangeException(nameof(baseCooldownSeconds), "Base cooldown must be finite and greater than zero.");
            ValidateNonNegativeFinite(cooldownMultiplier, nameof(cooldownMultiplier));
            if (!IsReady)
                throw new InvalidOperationException("Cooldown cannot be consumed before it is ready.");

            RemainingSeconds = baseCooldownSeconds * cooldownMultiplier;
        }

        private static void ValidateNonNegativeFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and non-negative.");
        }
    }
}

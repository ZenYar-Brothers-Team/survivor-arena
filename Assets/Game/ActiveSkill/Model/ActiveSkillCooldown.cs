using System;
using Game.Content;

namespace Game.ActiveSkill
{
    public sealed class ActiveSkillCooldown
    {
        public float RemainingSeconds { get; private set; }
        public bool IsReady => RemainingSeconds <= 0f;

        public void Tick(float deltaTime, bool isRunning)
        {
            NumericValidation.ValidateNonNegativeFinite(deltaTime, nameof(deltaTime));
            if (!isRunning || deltaTime == 0f || IsReady)
                return;

            RemainingSeconds = Math.Max(0f, RemainingSeconds - deltaTime);
        }

        public void Consume(float baseCooldownSeconds, float cooldownMultiplier)
        {
            NumericValidation.ValidatePositive(baseCooldownSeconds, nameof(baseCooldownSeconds), "Base cooldown must be finite and greater than zero.");
            NumericValidation.ValidateNonNegativeFinite(cooldownMultiplier, nameof(cooldownMultiplier));
            if (!IsReady)
                throw new InvalidOperationException("Cooldown cannot be consumed before it is ready.");

            RemainingSeconds = baseCooldownSeconds * cooldownMultiplier;
        }
    }
}

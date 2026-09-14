using System;
using System.Collections.Generic;

namespace Game.ActiveSkill
{
    public sealed class ActiveSkillLevelDefinition
    {
        public float BaseDamage { get; }
        public float CooldownSeconds { get; }
        public ActiveSkillTargetingMode TargetingMode { get; }
        public IReadOnlyList<ActiveSkillActivationWave> Waves { get; }

        public ActiveSkillLevelDefinition(
            float baseDamage,
            float cooldownSeconds,
            ActiveSkillTargetingMode targetingMode,
            params ActiveSkillActivationWave[] waves)
        {
            ProjectileBurstEffect.ValidateNonNegative(baseDamage, nameof(baseDamage));
            ProjectileBurstEffect.ValidatePositive(cooldownSeconds, nameof(cooldownSeconds));
            if (!Enum.IsDefined(typeof(ActiveSkillTargetingMode), targetingMode))
                throw new ArgumentOutOfRangeException(nameof(targetingMode));
            if (waves == null || waves.Length == 0)
                throw new ArgumentException("A skill level requires at least one activation wave.", nameof(waves));
            for (var i = 0; i < waves.Length; i++)
            {
                if (waves[i] == null)
                    throw new ArgumentException("Activation waves cannot contain null.", nameof(waves));
            }

            BaseDamage = baseDamage;
            CooldownSeconds = cooldownSeconds;
            TargetingMode = targetingMode;
            Waves = (ActiveSkillActivationWave[])waves.Clone();
        }
    }
}

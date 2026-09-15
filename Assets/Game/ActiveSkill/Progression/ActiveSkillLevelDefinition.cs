using System;
using System.Collections.Generic;
using Game.Content;
using Game.Presentation;

namespace Game.ActiveSkill
{
    public sealed class ActiveSkillLevelDefinition
    {
        public float BaseDamage { get; }
        public float CooldownSeconds { get; }
        public ActiveSkillTargetingMode TargetingMode { get; }
        public IReadOnlyList<ActiveSkillActivationWave> Waves { get; }

        // Optional: lets a level look distinct once real art exists (e.g. a
        // higher-tier variant at a later level) without changing its mechanics.
        // Unset by default, same convention as EnemyDefinition.Visual.
        public ContentRef<SpriteDefinition> Visual { get; }

        public ActiveSkillLevelDefinition(
            float baseDamage,
            float cooldownSeconds,
            ActiveSkillTargetingMode targetingMode,
            params ActiveSkillActivationWave[] waves)
            : this(baseDamage, cooldownSeconds, targetingMode, default, waves)
        {
        }

        public ActiveSkillLevelDefinition(
            float baseDamage,
            float cooldownSeconds,
            ActiveSkillTargetingMode targetingMode,
            ContentRef<SpriteDefinition> visual,
            params ActiveSkillActivationWave[] waves)
        {
            NumericValidation.ValidateNonNegativeFinite(baseDamage, nameof(baseDamage));
            NumericValidation.ValidatePositive(cooldownSeconds, nameof(cooldownSeconds));
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
            Visual = visual;
            Waves = (ActiveSkillActivationWave[])waves.Clone();
        }
    }
}

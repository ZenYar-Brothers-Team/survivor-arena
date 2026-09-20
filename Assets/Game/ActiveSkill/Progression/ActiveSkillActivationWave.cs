using System;
using System.Collections.Generic;
using Game.Content;
using Game.Combat;

namespace Game.ActiveSkill
{
    public sealed class ActiveSkillActivationWave
    {
        public float DelaySeconds { get; }
        public float RotationDegrees { get; }
        public float DamageMultiplier { get; }
        public CombatControlProfile Controls { get; }
        public IReadOnlyList<IActiveSkillEffect> Effects { get; }

        public ActiveSkillActivationWave(
            float delaySeconds,
            float rotationDegrees,
            float damageMultiplier,
            params IActiveSkillEffect[] effects)
            : this(delaySeconds, rotationDegrees, damageMultiplier, CombatControlProfile.None, effects) { }

        public ActiveSkillActivationWave(
            float delaySeconds,
            float rotationDegrees,
            float damageMultiplier,
            CombatControlProfile controls,
            params IActiveSkillEffect[] effects)
        {
            Controls = controls ?? throw new ArgumentNullException(nameof(controls));
            NumericValidation.ValidateNonNegativeFinite(delaySeconds, nameof(delaySeconds));
            if (float.IsNaN(rotationDegrees) || float.IsInfinity(rotationDegrees))
                throw new ArgumentOutOfRangeException(nameof(rotationDegrees));
            NumericValidation.ValidateNonNegativeFinite(damageMultiplier, nameof(damageMultiplier));
            if (effects == null || effects.Length == 0)
                throw new ArgumentException("An activation wave requires at least one effect.", nameof(effects));
            for (var i = 0; i < effects.Length; i++)
            {
                if (effects[i] == null)
                    throw new ArgumentException("Activation effects cannot contain null.", nameof(effects));
            }

            DelaySeconds = delaySeconds;
            RotationDegrees = rotationDegrees;
            DamageMultiplier = damageMultiplier;
            Effects = (IActiveSkillEffect[])effects.Clone();
        }
    }
}

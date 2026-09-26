using System;
using Game.Content;

namespace Game.ActiveSkill
{
    public sealed class ChainEffect : IActiveSkillEffect
    {
        public int TargetCount { get; }
        public float JumpRange { get; }
        public float DamageRetentionPerJump { get; }
        public float DamageMultiplier { get; }
        /// <summary>
        /// Fan-out: after the first target, up to TargetCount-1 other enemies within JumpRange of that first target are
        /// hit at once with DamageRetentionPerJump of its damage and never jump further (SET-013, DECISION-0061).
        /// </summary>
        public bool FanOut { get; }

        public ChainEffect(int targetCount, float jumpRange, float damageRetentionPerJump, float damageMultiplier = 1f, bool fanOut = false)
        {
            NumericValidation.ValidateCount(targetCount, nameof(targetCount));
            NumericValidation.ValidatePositive(jumpRange, nameof(jumpRange));
            NumericValidation.ValidateNonNegativeFinite(damageRetentionPerJump, nameof(damageRetentionPerJump));
            if (damageRetentionPerJump > 1f)
                throw new ArgumentOutOfRangeException(nameof(damageRetentionPerJump));
            NumericValidation.ValidateNonNegativeFinite(damageMultiplier, nameof(damageMultiplier));
            TargetCount = targetCount;
            JumpRange = jumpRange;
            DamageRetentionPerJump = damageRetentionPerJump;
            DamageMultiplier = damageMultiplier;
            FanOut = fanOut;
        }
    }
}

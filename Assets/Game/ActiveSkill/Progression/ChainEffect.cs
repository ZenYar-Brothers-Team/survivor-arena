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

        public ChainEffect(int targetCount, float jumpRange, float damageRetentionPerJump, float damageMultiplier = 1f)
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
        }
    }
}

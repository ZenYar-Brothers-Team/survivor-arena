using System;

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
            ProjectileBurstEffect.ValidateCount(targetCount, nameof(targetCount));
            ProjectileBurstEffect.ValidatePositive(jumpRange, nameof(jumpRange));
            ProjectileBurstEffect.ValidateNonNegative(damageRetentionPerJump, nameof(damageRetentionPerJump));
            if (damageRetentionPerJump > 1f)
                throw new ArgumentOutOfRangeException(nameof(damageRetentionPerJump));
            ProjectileBurstEffect.ValidateNonNegative(damageMultiplier, nameof(damageMultiplier));
            TargetCount = targetCount;
            JumpRange = jumpRange;
            DamageRetentionPerJump = damageRetentionPerJump;
            DamageMultiplier = damageMultiplier;
        }
    }
}

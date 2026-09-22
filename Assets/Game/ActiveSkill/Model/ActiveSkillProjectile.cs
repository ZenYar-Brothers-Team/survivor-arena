using System;
using Game.Content;
using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill
{
    public readonly struct ActiveSkillProjectile
    {
        public ProjectileBehavior Behavior { get; }
        public SkillHitLedger HitLedger { get; }
        public float HitCooldownSeconds { get; }
        public float ReturnKnockbackMultiplier { get; }
        public float RangeMultiplier { get; }
        public Vector2 Origin { get; }
        public Vector2 Direction { get; }
        public float Speed { get; }
        public float LifetimeSeconds { get; }
        public float CollisionRadius { get; }
        public float ImpactAreaRadius { get; }
        public EnemyDamageRequest Damage { get; }
        public int PierceCount { get; }
        public float ReturnAfterSeconds { get; }
        public float ReturnDamageMultiplier { get; }
        public Transform ReturnTarget { get; }
        public bool Returns => ReturnAfterSeconds > 0f;

        public ActiveSkillProjectile(
            Vector2 origin,
            Vector2 direction,
            float speed,
            float lifetimeSeconds,
            float collisionRadius,
            float impactAreaRadius,
            EnemyDamageRequest damage,
            int pierceCount = 0,
            float returnAfterSeconds = 0f,
            float returnDamageMultiplier = 1f,
            Transform returnTarget = null, ProjectileBehavior behavior = null,
            SkillHitLedger hitLedger = null, float hitCooldownSeconds = 0f, float returnKnockbackMultiplier = 1f, float rangeMultiplier = 1f)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                throw new ArgumentException("Projectile direction cannot be zero.", nameof(direction));
            NumericValidation.ValidatePositive(speed, nameof(speed));
            NumericValidation.ValidatePositive(lifetimeSeconds, nameof(lifetimeSeconds));
            NumericValidation.ValidatePositive(collisionRadius, nameof(collisionRadius));
            NumericValidation.ValidateNonNegativeFinite(impactAreaRadius, nameof(impactAreaRadius));
            if (pierceCount < 0)
                throw new ArgumentOutOfRangeException(nameof(pierceCount));
            NumericValidation.ValidateNonNegativeFinite(returnAfterSeconds, nameof(returnAfterSeconds));
            NumericValidation.ValidateNonNegativeFinite(returnDamageMultiplier, nameof(returnDamageMultiplier));
            if (returnAfterSeconds > 0f && returnTarget == null)
                throw new ArgumentNullException(nameof(returnTarget), "Returning projectiles require a return target.");

            NumericValidation.ValidateNonNegative(hitCooldownSeconds, nameof(hitCooldownSeconds));
            NumericValidation.ValidateNonNegative(returnKnockbackMultiplier, nameof(returnKnockbackMultiplier));
            NumericValidation.ValidatePositive(rangeMultiplier, nameof(rangeMultiplier));
            if (hitCooldownSeconds > 0f && hitLedger == null) throw new ArgumentNullException(nameof(hitLedger));
            Behavior = behavior ?? ProjectileBehavior.None;
            HitLedger = hitLedger;
            HitCooldownSeconds = hitCooldownSeconds;
            ReturnKnockbackMultiplier = returnKnockbackMultiplier;
            RangeMultiplier = rangeMultiplier;
            Origin = origin;
            Direction = direction.normalized;
            Speed = speed;
            LifetimeSeconds = lifetimeSeconds;
            CollisionRadius = collisionRadius;
            ImpactAreaRadius = impactAreaRadius;
            Damage = damage;
            PierceCount = pierceCount;
            ReturnAfterSeconds = returnAfterSeconds;
            ReturnDamageMultiplier = returnDamageMultiplier;
            ReturnTarget = returnTarget;
        }
    }
}

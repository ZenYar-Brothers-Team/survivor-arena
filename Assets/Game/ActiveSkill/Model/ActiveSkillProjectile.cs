using System;
using Game.Content;
using Game.Enemy;
using UnityEngine;

namespace Game.ActiveSkill
{
    public readonly struct ActiveSkillProjectile
    {
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
            Transform returnTarget = null)
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

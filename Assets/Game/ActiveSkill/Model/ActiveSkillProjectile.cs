using System;
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

        public ActiveSkillProjectile(
            Vector2 origin,
            Vector2 direction,
            float speed,
            float lifetimeSeconds,
            float collisionRadius,
            float impactAreaRadius,
            EnemyDamageRequest damage)
        {
            if (direction.sqrMagnitude <= Mathf.Epsilon)
                throw new ArgumentException("Projectile direction cannot be zero.", nameof(direction));
            ValidatePositive(speed, nameof(speed));
            ValidatePositive(lifetimeSeconds, nameof(lifetimeSeconds));
            ValidatePositive(collisionRadius, nameof(collisionRadius));
            ValidateNonNegative(impactAreaRadius, nameof(impactAreaRadius));

            Origin = origin;
            Direction = direction.normalized;
            Speed = speed;
            LifetimeSeconds = lifetimeSeconds;
            CollisionRadius = collisionRadius;
            ImpactAreaRadius = impactAreaRadius;
            Damage = damage;
        }

        private static void ValidatePositive(float value, string parameterName)
        {
            ValidateNonNegative(value, parameterName);
            if (value <= 0f)
                throw new ArgumentOutOfRangeException(parameterName, "Value must be greater than zero.");
        }

        private static void ValidateNonNegative(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value) || value < 0f)
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite and non-negative.");
        }
    }
}

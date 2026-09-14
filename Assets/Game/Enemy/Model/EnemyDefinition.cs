using System;
using Game.Content;

namespace Game.Enemy
{
    public sealed class EnemyDefinition : IContentDefinition
    {
        public ContentId Id { get; }
        public float MaxHealth { get; }
        public float CollisionSize { get; }
        public float MovementSpeed { get; }
        public float ContactDamage { get; }
        public float ContactDamageInterval { get; }
        public float ExperienceReward { get; }

        public EnemyDefinition(
            ContentId id,
            float maxHealth,
            float collisionSize,
            float movementSpeed,
            float contactDamage,
            float contactDamageInterval,
            float experienceReward = 0f)
        {
            if (!id.IsValid)
                throw new ArgumentException("Enemy definition requires a valid content id.", nameof(id));

            ValidatePositive(maxHealth, nameof(maxHealth));
            ValidatePositive(collisionSize, nameof(collisionSize));
            ValidateNonNegative(movementSpeed, nameof(movementSpeed));
            ValidateNonNegative(contactDamage, nameof(contactDamage));
            ValidatePositive(contactDamageInterval, nameof(contactDamageInterval));
            ValidateNonNegative(experienceReward, nameof(experienceReward));

            Id = id;
            MaxHealth = maxHealth;
            CollisionSize = collisionSize;
            MovementSpeed = movementSpeed;
            ContactDamage = contactDamage;
            ContactDamageInterval = contactDamageInterval;
            ExperienceReward = experienceReward;
        }

        private static void ValidatePositive(float value, string parameterName)
        {
            ValidateFinite(value, parameterName);
            if (value <= 0f)
                throw new ArgumentOutOfRangeException(parameterName, "Value must be greater than zero.");
        }

        private static void ValidateNonNegative(float value, string parameterName)
        {
            ValidateFinite(value, parameterName);
            if (value < 0f)
                throw new ArgumentOutOfRangeException(parameterName, "Value cannot be negative.");
        }

        private static void ValidateFinite(float value, string parameterName)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
                throw new ArgumentOutOfRangeException(parameterName, "Value must be finite.");
        }
    }
}

using System;
using System.Collections.Generic;
using Game.Content;
using Game.Presentation;

namespace Game.Enemy
{
    public sealed class EnemyDefinition : IContentDefinition, IReferencesContent
    {
        public ContentId Id { get; }
        public float MaxHealth { get; }
        public float CollisionSize { get; }
        public float MovementSpeed { get; }
        public float ContactDamage { get; }
        public float ContactDamageInterval { get; }
        public float ExperienceReward { get; }
        public ContentRef<SpriteDefinition> Visual { get; }

        public EnemyDefinition(
            ContentId id,
            float maxHealth,
            float collisionSize,
            float movementSpeed,
            float contactDamage,
            float contactDamageInterval,
            float experienceReward = 0f,
            ContentRef<SpriteDefinition> visual = default)
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
            Visual = visual;
        }

        // Visual is optional: content authored without art yet (e.g. fixtures) simply
        // doesn't declare a reference, so the registry has nothing to validate for it.
        public IEnumerable<ContentReference> GetReferencedContent()
        {
            if (Visual.Id.IsValid)
                yield return Visual.ToReference();
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

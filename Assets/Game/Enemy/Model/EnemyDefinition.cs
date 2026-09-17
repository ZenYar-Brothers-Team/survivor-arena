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

            NumericValidation.ValidatePositive(maxHealth, nameof(maxHealth));
            NumericValidation.ValidatePositive(collisionSize, nameof(collisionSize));
            NumericValidation.ValidateNonNegative(movementSpeed, nameof(movementSpeed));
            NumericValidation.ValidateNonNegative(contactDamage, nameof(contactDamage));
            NumericValidation.ValidatePositive(contactDamageInterval, nameof(contactDamageInterval));
            NumericValidation.ValidateNonNegative(experienceReward, nameof(experienceReward));

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
    }
}

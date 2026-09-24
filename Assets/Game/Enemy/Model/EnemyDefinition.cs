using System;
using System.Collections.Generic;
using Game.Content;
using Game.Combat;
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
        public ContentRef<SpriteMotionProfile> MotionProfile { get; }
        public EnemyMovementProfile Movement { get; }
        public EnemyAttackProfile Attack { get; }
        public float KnockbackResistance { get; }
        public CombatControlProfile DashContactControls { get; }
        public CombatControlProfile ContactControls { get; }

        public EnemyDefinition(
            ContentId id,
            float maxHealth,
            float collisionSize,
            float movementSpeed,
            float contactDamage,
            float contactDamageInterval,
            float experienceReward = 0f,
            ContentRef<SpriteDefinition> visual = default,
            EnemyMovementProfile movement = null,
            EnemyAttackProfile attack = null,
            float knockbackResistance = 0f,
            CombatControlProfile contactControls = null,
            CombatControlProfile dashContactControls = null,
            ContentRef<SpriteMotionProfile> motionProfile = default)
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
            if (motionProfile.Id.IsValid && !visual.Id.IsValid)
                throw new ArgumentException("Enemy motion requires a body visual.", nameof(motionProfile));
            MotionProfile = motionProfile;
            Movement = movement ?? EnemyMovementProfile.Seek;
            Attack = attack;
            NumericValidation.ValidateRange(knockbackResistance, 0f, 1f, nameof(knockbackResistance));
            KnockbackResistance = knockbackResistance;
            ContactControls = contactControls ?? CombatControlProfile.None;
            DashContactControls = dashContactControls ?? CombatControlProfile.None;
        }

        // Visual is optional: content authored without art yet (e.g. fixtures) simply
        // doesn't declare a reference, so the registry has nothing to validate for it.
        public IEnumerable<ContentReference> GetReferencedContent()
        {
            if (Visual.Id.IsValid)
                yield return Visual.ToReference();
            if (MotionProfile.Id.IsValid)
                yield return MotionProfile.ToReference();
            if (Attack?.ProjectileVisual.Id.IsValid == true)
                yield return Attack.ProjectileVisual.ToReference();
        }
    }
}

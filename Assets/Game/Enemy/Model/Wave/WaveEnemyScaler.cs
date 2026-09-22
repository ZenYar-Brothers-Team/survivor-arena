using System;

namespace Game.Enemy
{
    public static class WaveEnemyScaler
    {
        // Returns the original definition for identity modifiers so the common case
        // allocates nothing; otherwise a same-id copy with the phase's multipliers.
        public static EnemyDefinition Apply(EnemyDefinition definition, WaveEnemyModifiers modifiers)
        {
            if (definition == null)
                throw new ArgumentNullException(nameof(definition));
            if (modifiers == null)
                throw new ArgumentNullException(nameof(modifiers));
            if (modifiers.IsIdentity)
                return definition;

            return new EnemyDefinition(
                definition.Id,
                definition.MaxHealth * modifiers.HealthMultiplier,
                definition.CollisionSize,
                definition.MovementSpeed * modifiers.SpeedMultiplier,
                definition.ContactDamage * modifiers.ContactDamageMultiplier,
                definition.ContactDamageInterval,
                definition.ExperienceReward,
                definition.Visual,
                definition.Movement,
                ScaleAttack(definition.Attack, modifiers.AttackDamageMultiplier),
                definition.KnockbackResistance,
                definition.ContactControls, definition.DashContactControls, definition.MotionProfile);
        }

        private static EnemyAttackProfile ScaleAttack(EnemyAttackProfile attack, float damageMultiplier)
        {
            if (attack == null)
                return null;

            return new EnemyAttackProfile(
                attack.Pattern,
                attack.Damage * damageMultiplier,
                attack.CooldownSeconds,
                attack.ProjectileSpeed,
                attack.ProjectileLifetimeSeconds,
                attack.ProjectileCount,
                attack.SpreadDegrees,
                attack.BurstIntervalSeconds,
                attack.ProjectileRadius,
                attack.ExplosionRadius,
                attack.RotationStepDegrees,
                attack.Controls, attack.TelegraphSeconds, attack.ProjectileVisual);
        }
    }
}

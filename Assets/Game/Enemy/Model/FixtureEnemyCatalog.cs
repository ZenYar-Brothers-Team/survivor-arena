using System;
using System.Collections.Generic;
using Game.Content;
using Game.Content.Json;
using Game.Enemy.Json;
using Game.Presentation;

namespace Game.Enemy
{
    // Non-production enemy content, config-driven instead of hardcoded: see
    // Resources/Content/Enemies/FixtureEnemies.json and AGENTS.md's content-config rule.
    public static class FixtureEnemyCatalog
    {
        private const string ResourcePath = "Content/Enemies/FixtureEnemies";

        public static IReadOnlyList<EnemyDefinition> Create()
        {
            var data = JsonContentFile.Load<EnemyDefinitionData[]>(ResourcePath);
            var definitions = new EnemyDefinition[data.Length];
            for (var i = 0; i < data.Length; i++)
                definitions[i] = ToDefinition(data[i]);

            return definitions;
        }

        private static EnemyDefinition ToDefinition(EnemyDefinitionData data)
        {
            var visual = string.IsNullOrEmpty(data.VisualId)
                ? default
                : new ContentRef<SpriteDefinition>(data.VisualId);

            return new EnemyDefinition(
                data.Id,
                data.MaxHealth,
                data.CollisionSize,
                data.MovementSpeed,
                data.ContactDamage,
                data.ContactDamageInterval,
                data.ExperienceReward,
                visual,
                ToMovement(data.Movement),
                ToAttack(data.Attack));
        }

        private static EnemyMovementProfile ToMovement(EnemyMovementProfileData data)
        {
            if (data == null)
                return EnemyMovementProfile.Seek;
            if (!Enum.TryParse(data.Kind, true, out EnemyMovementKind kind))
                throw new InvalidOperationException($"Unknown enemy movement kind '{data.Kind}'.");
            return new EnemyMovementProfile(
                kind,
                data.PreferredDistance,
                data.DistanceTolerance,
                data.LateralStrength,
                data.CycleSeconds,
                data.DashTelegraphSeconds,
                data.DashDurationSeconds,
                data.DashCooldownSeconds,
                data.DashSpeedMultiplier);
        }

        private static EnemyAttackProfile ToAttack(EnemyAttackProfileData data)
        {
            if (data == null)
                return null;
            if (!Enum.TryParse(data.Pattern, true, out EnemyProjectilePattern pattern))
                throw new InvalidOperationException($"Unknown enemy projectile pattern '{data.Pattern}'.");
            return new EnemyAttackProfile(
                pattern,
                data.Damage,
                data.CooldownSeconds,
                data.ProjectileSpeed,
                data.ProjectileLifetimeSeconds,
                data.ProjectileCount,
                data.SpreadDegrees,
                data.BurstIntervalSeconds,
                data.ProjectileRadius,
                data.ExplosionRadius,
                data.RotationStepDegrees);
        }
    }
}

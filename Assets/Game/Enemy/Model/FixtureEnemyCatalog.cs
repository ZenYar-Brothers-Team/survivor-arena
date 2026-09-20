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
                ToMovement(data.Id, data.Movement),
                ToAttack(data.Id, data.Attack));
        }

        // Every field the kind actually reads must be explicit in config; fields it never
        // reads fall back to the domain profile's neutral values and cannot affect behavior.
        private static EnemyMovementProfile ToMovement(string enemyId, EnemyMovementProfileData data)
        {
            if (data == null)
                return EnemyMovementProfile.Seek;
            if (!Enum.TryParse(data.Kind, true, out EnemyMovementKind kind))
                throw new InvalidOperationException($"Unknown enemy movement kind '{data.Kind}'.");

            var neutral = new EnemyMovementProfile(kind);
            var usesDistance = kind == EnemyMovementKind.KeepDistance || kind == EnemyMovementKind.Orbit;
            var usesLateral = kind == EnemyMovementKind.Orbit || kind == EnemyMovementKind.Zigzag;
            var usesCycle = kind == EnemyMovementKind.Zigzag || kind == EnemyMovementKind.ApproachRetreat;
            var usesDash = kind == EnemyMovementKind.TelegraphedDash;

            string Owner(string field) => $"Enemy '{enemyId}' movement '{kind}' field '{field}'";
            return new EnemyMovementProfile(
                kind,
                Pick(data.PreferredDistance, usesDistance, neutral.PreferredDistance, Owner(nameof(data.PreferredDistance))),
                Pick(data.DistanceTolerance, usesDistance, neutral.DistanceTolerance, Owner(nameof(data.DistanceTolerance))),
                Pick(data.LateralStrength, usesLateral, neutral.LateralStrength, Owner(nameof(data.LateralStrength))),
                Pick(data.CycleSeconds, usesCycle, neutral.CycleSeconds, Owner(nameof(data.CycleSeconds))),
                Pick(data.DashTelegraphSeconds, usesDash, neutral.DashTelegraphSeconds, Owner(nameof(data.DashTelegraphSeconds))),
                Pick(data.DashDurationSeconds, usesDash, neutral.DashDurationSeconds, Owner(nameof(data.DashDurationSeconds))),
                Pick(data.DashCooldownSeconds, usesDash, neutral.DashCooldownSeconds, Owner(nameof(data.DashCooldownSeconds))),
                Pick(data.DashSpeedMultiplier, usesDash, neutral.DashSpeedMultiplier, Owner(nameof(data.DashSpeedMultiplier))));
        }

        private static EnemyAttackProfile ToAttack(string enemyId, EnemyAttackProfileData data)
        {
            if (data == null)
                return null;
            if (!Enum.TryParse(data.Pattern, true, out EnemyProjectilePattern pattern))
                throw new InvalidOperationException($"Unknown enemy projectile pattern '{data.Pattern}'.");

            string Owner(string field) => $"Enemy '{enemyId}' attack '{pattern}' field '{field}'";
            var projectileCount = Require(data.ProjectileCount, Owner(nameof(data.ProjectileCount)));
            var projectileRadius = Require(data.ProjectileRadius, Owner(nameof(data.ProjectileRadius)));

            if (pattern != EnemyProjectilePattern.Burst)
            {
                return new EnemyAttackProfile(
                    pattern,
                    data.Damage,
                    data.CooldownSeconds,
                    data.ProjectileSpeed,
                    data.ProjectileLifetimeSeconds,
                    projectileCount,
                    data.SpreadDegrees,
                    projectileRadius: projectileRadius,
                    explosionRadius: data.ExplosionRadius,
                    rotationStepDegrees: data.RotationStepDegrees);
            }

            return new EnemyAttackProfile(
                pattern,
                data.Damage,
                data.CooldownSeconds,
                data.ProjectileSpeed,
                data.ProjectileLifetimeSeconds,
                projectileCount,
                data.SpreadDegrees,
                Require(data.BurstIntervalSeconds, Owner(nameof(data.BurstIntervalSeconds))),
                projectileRadius,
                data.ExplosionRadius,
                data.RotationStepDegrees);
        }

        private static float Pick(float? configured, bool required, float unusedFallback, string description)
        {
            return required ? Require(configured, description) : configured ?? unusedFallback;
        }

        private static T Require<T>(T? configured, string description) where T : struct
        {
            if (!configured.HasValue)
                throw new InvalidOperationException($"{description} must be set in config.");
            return configured.Value;
        }
    }
}

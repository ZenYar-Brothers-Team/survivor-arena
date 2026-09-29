using System;
using Game.Combat;
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

        public static IReadOnlyList<EnemyDefinition> Create() => Load(ResourcePath);

        /// <summary>Loads any enemy JSON (fixture or production) with the shared DTO validation.</summary>
        public static IReadOnlyList<EnemyDefinition> Load(string resourcePath)
        {
            var data = JsonContentFile.Load<EnemyDefinitionData[]>(resourcePath);
            var definitions = new EnemyDefinition[data.Length];
            for (var i = 0; i < data.Length; i++)
                definitions[i] = ToDefinition(data[i]);

            return definitions;
        }

        public static EnemyDefinition ToDefinition(EnemyDefinitionData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var movement = ToMovement(data.Id, data.Movement);
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
                movement,
                ToAttack(data.Id, data.Attack),
                Require(data.KnockbackResistance, $"Enemy {data.Id} knockbackResistance"),
                RequireControls(data.ContactControls, $"Enemy {data.Id} contactControls"),
                movement.Kind == EnemyMovementKind.TelegraphedDash
                    ? RequireControls(data.DashContactControls, $"Enemy {data.Id} dashContactControls") : null,
                string.IsNullOrEmpty(data.MotionProfileId) ? default :
                    new ContentRef<SpriteMotionProfile>(data.MotionProfileId),
                ToDashVolley(data),
                ToMovementVariants(data));
        }

        private static EnemyMovementVariant[] ToMovementVariants(EnemyDefinitionData data)
        {
            if (data.MovementVariants == null) return Array.Empty<EnemyMovementVariant>();
            var variants = new EnemyMovementVariant[data.MovementVariants.Length];
            for (var i = 0; i < variants.Length; i++)
            {
                var entry = data.MovementVariants[i] ??
                    throw new InvalidOperationException($"Enemy {data.Id} movementVariants[{i}] is empty.");
                if (entry.Movement == null)
                    throw new InvalidOperationException($"Enemy {data.Id} movementVariants[{i}].movement must be set in config.");
                variants[i] = new EnemyMovementVariant(
                    Require(entry.Chance, $"Enemy {data.Id} movementVariants[{i}].chance"),
                    ToMovement($"{data.Id} movementVariants[{i}]", entry.Movement));
            }
            return variants;
        }

        private static EnemyDashVolleyProfile ToDashVolley(EnemyDefinitionData data)
        {
            if (data.DashEndAttacks != null)
            {
                if (data.DashEndAttack != null || data.DashEndRepeat != null)
                    throw new InvalidOperationException($"Enemy {data.Id} dashEndAttacks excludes dashEndAttack/dashEndRepeat.");
                var replacement = data.DashEndReplacement;
                return new EnemyDashVolleyProfile(ToDashEntries(data.Id, data.DashEndAttacks, "dashEndAttacks"),
                    replacement == null ? 0f : Require(replacement.BelowHealthFraction, $"Enemy {data.Id} dashEndReplacement.belowHealthFraction"),
                    replacement == null ? null : ToDashEntries(data.Id, replacement.Attacks, "dashEndReplacement.attacks"));
            }
            if (data.DashEndReplacement != null) throw new InvalidOperationException($"Enemy {data.Id} dashEndReplacement needs dashEndAttacks.");
            if (data.DashEndAttack == null)
            {
                if (data.DashEndRepeat != null) throw new InvalidOperationException($"Enemy {data.Id} dashEndRepeat needs dashEndAttack.");
                return null;
            }
            var attack = ToAttack(data.Id, data.DashEndAttack);
            var repeat = data.DashEndRepeat;
            return repeat == null ? new EnemyDashVolleyProfile(attack) : new EnemyDashVolleyProfile(attack,
                Require(repeat.EveryNthDash, $"Enemy {data.Id} dashEndRepeat.everyNthDash"),
                Require(repeat.BelowHealthFraction, $"Enemy {data.Id} dashEndRepeat.belowHealthFraction"),
                Require(repeat.DelaySeconds, $"Enemy {data.Id} dashEndRepeat.delaySeconds"),
                Require(repeat.RotationDegrees, $"Enemy {data.Id} dashEndRepeat.rotationDegrees"));
        }

        private static EnemyDashVolleyEntry[] ToDashEntries(string enemyId, EnemyDashVolleyEntryData[] entries, string field)
        {
            if (entries == null) throw new InvalidOperationException($"Enemy {enemyId} {field} must be set in config.");
            var result = new EnemyDashVolleyEntry[entries.Length];
            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i] ?? throw new InvalidOperationException($"Enemy {enemyId} {field}[{i}] is empty.");
                var owner = $"Enemy {enemyId} {field}[{i}]";
                if (!Enum.TryParse(entry.Orientation, false, out EnemyDashVolleyOrientation orientation) ||
                    !Enum.IsDefined(typeof(EnemyDashVolleyOrientation), orientation))
                    throw new InvalidOperationException($"{owner}.orientation '{entry.Orientation}' is not a known orientation.");
                result[i] = new EnemyDashVolleyEntry(Require(entry.DelaySeconds, $"{owner}.delaySeconds"), orientation,
                    ToAttack(enemyId, entry.Attack), entry.Zone == null ? null : BossSpecialCatalog.ToZone(entry.Zone, $"{owner}.zone"));
            }
            return result;
        }

        /// <summary>Movement authoring shared with boss phase overrides (DECISION-0066, E3).</summary>
        public static EnemyMovementProfile ToMovementProfile(string ownerId, EnemyMovementProfileData data) => ToMovement(ownerId, data);

        // Every field the kind actually reads must be explicit in config; fields it never
        // reads fall back to the domain profile's neutral values and cannot affect behavior.
        private static EnemyMovementProfile ToMovement(string enemyId, EnemyMovementProfileData data)
        {
            if (data == null)
                return EnemyMovementProfile.Seek;
            if (!Enum.TryParse(data.Kind, true, out EnemyMovementKind kind))
                throw new InvalidOperationException($"Unknown enemy movement kind '{data.Kind}'.");

            // Neutral fallbacks for fields this kind never reads (Seek carries every domain default).
            var neutral = EnemyMovementProfile.Seek;
            var reposition = kind == EnemyMovementKind.DistanceReposition;
            var offsetPursuit = kind == EnemyMovementKind.OffsetPursuit;
            var blockedSidestep = kind == EnemyMovementKind.BlockedSidestep;
            var arcPass = kind == EnemyMovementKind.ArcPassPursuit;
            var inertial = kind == EnemyMovementKind.InertialPursuit;
            var usesDistance = kind == EnemyMovementKind.KeepDistance || kind == EnemyMovementKind.Orbit ||
                               reposition || offsetPursuit || blockedSidestep || arcPass;
            var usesTolerance = kind == EnemyMovementKind.KeepDistance || kind == EnemyMovementKind.Orbit ||
                                reposition || offsetPursuit;
            var usesLateral = kind == EnemyMovementKind.Orbit || kind == EnemyMovementKind.Zigzag ||
                              reposition || blockedSidestep || arcPass;
            var usesCycle = kind == EnemyMovementKind.Zigzag || kind == EnemyMovementKind.ApproachRetreat ||
                            kind == EnemyMovementKind.CommittedPursuit || offsetPursuit || arcPass || reposition;
            var usesDash = kind == EnemyMovementKind.TelegraphedDash;

            string Owner(string field) => $"Enemy '{enemyId}' movement '{kind}' field '{field}'";
            return new EnemyMovementProfile(
                kind,
                Pick(data.PreferredDistance, usesDistance, neutral.PreferredDistance, Owner(nameof(data.PreferredDistance))),
                Pick(data.DistanceTolerance, usesTolerance, neutral.DistanceTolerance, Owner(nameof(data.DistanceTolerance))),
                Pick(data.LateralStrength, usesLateral, neutral.LateralStrength, Owner(nameof(data.LateralStrength))),
                Pick(data.CycleSeconds, usesCycle, neutral.CycleSeconds, Owner(nameof(data.CycleSeconds))),
                Pick(data.DashTelegraphSeconds, usesDash, neutral.DashTelegraphSeconds, Owner(nameof(data.DashTelegraphSeconds))),
                Pick(data.DashDurationSeconds, usesDash, neutral.DashDurationSeconds, Owner(nameof(data.DashDurationSeconds))),
                Pick(data.DashCooldownSeconds, usesDash, neutral.DashCooldownSeconds, Owner(nameof(data.DashCooldownSeconds))),
                Pick(data.DashSpeedMultiplier, usesDash, neutral.DashSpeedMultiplier, Owner(nameof(data.DashSpeedMultiplier))),
                Pick(data.RepositionSeconds, reposition, neutral.RepositionSeconds, Owner(nameof(data.RepositionSeconds))),
                usesDash ? data.DashCount ?? 1 : 1,
                usesDash && (data.DashCount ?? 1) > 1
                    ? Require(data.FollowUpTelegraphSeconds, Owner(nameof(data.FollowUpTelegraphSeconds)))
                    : data.FollowUpTelegraphSeconds ?? 0f,
                usesDash ? data.ShowDashTelegraphLine ?? neutral.ShowDashTelegraphLine : neutral.ShowDashTelegraphLine,
                Pick(data.DirectPursuitSeconds, offsetPursuit || arcPass, neutral.DirectPursuitSeconds,
                    Owner(nameof(data.DirectPursuitSeconds))),
                Pick(data.BlockedTriggerSeconds, blockedSidestep, neutral.BlockedTriggerSeconds,
                    Owner(nameof(data.BlockedTriggerSeconds))),
                Pick(data.BlockedProgressFraction, blockedSidestep, neutral.BlockedProgressFraction,
                    Owner(nameof(data.BlockedProgressFraction))),
                Pick(data.SidestepSeconds, blockedSidestep, neutral.SidestepSeconds,
                    Owner(nameof(data.SidestepSeconds))),
                Pick(data.SidestepCooldownSeconds, blockedSidestep, neutral.SidestepCooldownSeconds,
                    Owner(nameof(data.SidestepCooldownSeconds))),
                Pick(data.TurnResponseSeconds, inertial, neutral.TurnResponseSeconds,
                    Owner(nameof(data.TurnResponseSeconds))),
                Pick(data.SidestepNearDistance, blockedSidestep, neutral.SidestepNearDistance,
                    Owner(nameof(data.SidestepNearDistance))),
                Pick(data.SidestepNearSeconds, blockedSidestep, neutral.SidestepNearSeconds,
                    Owner(nameof(data.SidestepNearSeconds))));
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

            var fanSpread = pattern == EnemyProjectilePattern.Fan || (pattern == EnemyProjectilePattern.Explosive && projectileCount > 1);
            var followUps = new List<EnemyAttackFollowUp>();
            foreach (var followUp in data.FollowUps ?? Array.Empty<EnemyAttackFollowUpData>())
            {
                if (followUp == null) throw new InvalidOperationException($"{Owner(nameof(data.FollowUps))} entries cannot be empty.");
                followUps.Add(new EnemyAttackFollowUp(Require(followUp.DelaySeconds, Owner("followUps.delaySeconds")),
                    Require(followUp.RotationDegrees, Owner("followUps.rotationDegrees"))));
            }
            return new EnemyAttackProfile(
                pattern, data.Damage, data.CooldownSeconds, data.ProjectileSpeed, data.ProjectileLifetimeSeconds,
                projectileCount,
                Pick(data.SpreadDegrees, fanSpread, 0f, Owner(nameof(data.SpreadDegrees))),
                pattern == EnemyProjectilePattern.Burst ? Require(data.BurstIntervalSeconds, Owner(nameof(data.BurstIntervalSeconds))) : 1f,
                projectileRadius,
                Pick(data.ExplosionRadius, pattern == EnemyProjectilePattern.Explosive, 0f, Owner(nameof(data.ExplosionRadius))),
                Pick(data.RotationStepDegrees, pattern == EnemyProjectilePattern.Spiral, 0f, Owner(nameof(data.RotationStepDegrees))),
                RequireControls(data.Controls, Owner(nameof(data.Controls))),
                Require(data.TelegraphSeconds, Owner(nameof(data.TelegraphSeconds))),
                string.IsNullOrWhiteSpace(data.ProjectileVisualId)
                    ? default : new ContentRef<Game.Presentation.SpriteDefinition>(data.ProjectileVisualId),
                ParseCadence(data.Cadence, Owner(nameof(data.Cadence))),
                data.FixedOrientation ?? false,
                followUps,
                data.WindupMovementMultiplier ?? 1f);
        }

        private static EnemyAttackCadence ParseCadence(string cadence, string owner)
        {
            if (string.IsNullOrWhiteSpace(cadence)) return EnemyAttackCadence.CooldownAfterShot;
            if (!Enum.TryParse(cadence, false, out EnemyAttackCadence parsed) || !Enum.IsDefined(typeof(EnemyAttackCadence), parsed))
                throw new InvalidOperationException($"{owner} '{cadence}' is not a known cadence.");
            return parsed;
        }

        private static CombatControlProfile RequireControls(CombatControlData data, string owner)
        {
            if (data?.KnockbackDistance == null) throw new InvalidOperationException($"{owner}.knockbackDistance must be explicit, including zero.");
            return data.ToProfile();
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

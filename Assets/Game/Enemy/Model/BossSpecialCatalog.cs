using System;
using Game.Combat;
using Game.Enemy.Json;

namespace Game.Enemy
{
    /// <summary>
    /// Maps boss zone/beam/summon JSON (DECISION-0066) to validated profiles. Every field a placement or family reads is
    /// required; fields it never reads fall back to neutral zero and cannot change behavior.
    /// </summary>
    public static class BossSpecialCatalog
    {
        public static BossSpecialAttack ToSpecial(BossAttackData data, string owner, Func<string, EnemyDefinition> resolveEnemy)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            var cooldown = Require(data.CooldownSeconds, $"{owner}.cooldownSeconds");
            return new BossSpecialAttack(cooldown,
                data.Zone == null ? null : ToZone(data.Zone, $"{owner}.zone"),
                data.Beam == null ? null : ToBeam(data.Beam, $"{owner}.beam"),
                data.Summon == null ? null : ToSummon(data.Summon, $"{owner}.summon", resolveEnemy));
        }

        public static bool IsSpecial(BossAttackData data) => data.Zone != null || data.Beam != null || data.Summon != null;

        public static BossZoneProfile ToZone(BossZoneData data, string owner)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (!Enum.TryParse(data.Placement, false, out BossZonePlacement placement) || !Enum.IsDefined(typeof(BossZonePlacement), placement))
                throw new InvalidOperationException($"{owner}.placement '{data.Placement}' is not a known placement.");
            var scattered = placement == BossZonePlacement.AroundPlayer || placement == BossZonePlacement.SafeCircles;
            var trail = placement == BossZonePlacement.Trail;
            var linger = Require(data.LingerSeconds, $"{owner}.lingerSeconds");
            return new BossZoneProfile(placement,
                Require(data.Count, $"{owner}.count"),
                Require(data.Radius, $"{owner}.radius"),
                Pick(data.ScatterRadius, scattered, $"{owner}.scatterRadius"),
                Pick(data.MinSpacing, scattered, $"{owner}.minSpacing"),
                Pick(data.IntervalSeconds, trail, $"{owner}.intervalSeconds"),
                Require(data.FillSeconds, $"{owner}.fillSeconds"),
                Require(data.Damage, $"{owner}.damage"),
                RequireControls(data.Controls, $"{owner}.controls"),
                linger,
                Pick(data.LingerDamagePerSecond, linger > 0f, $"{owner}.lingerDamagePerSecond"),
                Pick(data.LingerTickSeconds, linger > 0f, $"{owner}.lingerTickSeconds"),
                Require(data.ImpactEffectSeconds, $"{owner}.impactEffectSeconds"),
                BossHazardColor.Require(data.TelegraphColor, $"{owner}.telegraphColor"),
                BossHazardColor.Require(data.ImpactColor, $"{owner}.impactColor"));
        }

        public static BossBeamProfile ToBeam(BossBeamData data, string owner)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (data.AnglesDegrees == null) throw new InvalidOperationException($"{owner}.anglesDegrees must be set in config.");
            return new BossBeamProfile(data.AnglesDegrees,
                Require(data.Length, $"{owner}.length"),
                Require(data.Width, $"{owner}.width"),
                Require(data.TelegraphSeconds, $"{owner}.telegraphSeconds"),
                Require(data.ActiveSeconds, $"{owner}.activeSeconds"),
                Require(data.SweepDegrees, $"{owner}.sweepDegrees"),
                Require(data.Damage, $"{owner}.damage"),
                RequireControls(data.Controls, $"{owner}.controls"),
                BossHazardColor.Require(data.TelegraphColor, $"{owner}.telegraphColor"),
                BossHazardColor.Require(data.BeamColor, $"{owner}.beamColor"));
        }

        public static BossSummonProfile ToSummon(BossSummonData data, string owner, Func<string, EnemyDefinition> resolveEnemy)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            if (resolveEnemy == null) throw new ArgumentNullException(nameof(resolveEnemy));
            if (string.IsNullOrWhiteSpace(data.EnemyId)) throw new InvalidOperationException($"{owner}.enemyId must be set in config.");
            var enemy = resolveEnemy(data.EnemyId) ?? throw new InvalidOperationException($"{owner}.enemyId '{data.EnemyId}' is unknown.");
            return new BossSummonProfile(enemy,
                Require(data.Count, $"{owner}.count"),
                Require(data.SpawnDistance, $"{owner}.spawnDistance"),
                Require(data.MaxAlive, $"{owner}.maxAlive"),
                Require(data.TelegraphSeconds, $"{owner}.telegraphSeconds"),
                BossHazardColor.Require(data.MarkerColor, $"{owner}.markerColor"));
        }

        private static CombatControlProfile RequireControls(CombatControlData data, string owner)
        {
            if (data?.KnockbackDistance == null) throw new InvalidOperationException($"{owner}.knockbackDistance must be explicit, including zero.");
            return data.ToProfile();
        }

        private static float Pick(float? value, bool required, string owner) => required ? Require(value, owner) : value ?? 0f;

        private static T Require<T>(T? value, string owner) where T : struct =>
            value ?? throw new InvalidOperationException($"{owner} must be set in config.");
    }
}

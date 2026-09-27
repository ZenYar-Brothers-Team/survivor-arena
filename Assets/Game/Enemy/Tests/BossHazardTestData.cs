using Game.Combat;
using UnityEngine;

namespace Game.Enemy.Tests
{
    /// <summary>Builders for DECISION-0066 boss hazard profiles used by several test classes.</summary>
    internal static class BossHazardTestData
    {
        public static readonly Color Color = new Color(1f, .5f, .2f, .8f);

        public static BossZoneProfile Zone(BossZonePlacement placement, int count = 1, float radius = 1f, float fill = 1f,
            float scatter = 0f, float spacing = 0f, float interval = 0f, float damage = 20f, float linger = 0f,
            float lingerDps = 0f, float tick = 0f, CombatControlProfile controls = null) =>
            new BossZoneProfile(placement, count, radius, scatter, spacing, interval, fill, damage,
                controls ?? new CombatControlProfile(0.3f, 0.12f), linger, lingerDps, tick, 0.45f, Color, Color);

        public static BossBeamProfile Beam(float[] angles = null, float width = 1f, float telegraph = 1f, float active = 0.5f,
            float sweep = 0f, float damage = 30f) =>
            new BossBeamProfile(angles ?? new[] { 0f }, 24f, width, telegraph, active, sweep, damage,
                new CombatControlProfile(0.4f, 0.12f), Color, Color);

        public static EnemyDefinition Minion(string id = "FIXTURE-SUMMON") => new EnemyDefinition(id, 10f, 0.8f, 1f, 1f, 1f);

        public static BossSummonProfile Summon(int count = 2, int maxAlive = 4, EnemyDefinition enemy = null) =>
            new BossSummonProfile(enemy ?? Minion(), count, 6f, maxAlive, 0.8f, Color);
    }
}

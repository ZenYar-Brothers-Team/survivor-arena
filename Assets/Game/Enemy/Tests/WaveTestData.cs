using System.Collections.Generic;
using Game.Content;

namespace Game.Enemy.Tests
{
    internal static class WaveTestData
    {
        public const float SpawnRadius = 6f;

        public static EnemyDefinition Enemy(string id, float health = 10f, float speed = 1f, float contactDamage = 1f) =>
            new EnemyDefinition(id, health, 1f, speed, contactDamage, 1f);

        public static Dictionary<ContentId, EnemyDefinition> Enemies(params EnemyDefinition[] enemies)
        {
            var map = new Dictionary<ContentId, EnemyDefinition>();
            for (var i = 0; i < enemies.Length; i++)
                map.Add(enemies[i].Id, enemies[i]);
            return map;
        }

        public static WaveCompositionEntry Entry(string enemyId, float weight = 1f) =>
            new WaveCompositionEntry(enemyId, weight);

        public static WavePhaseDefinition Phase(
            string id,
            WavePhaseTag tag,
            float duration,
            float interval,
            int maxAlive,
            WaveEnemyModifiers modifiers,
            params WaveCompositionEntry[] composition) =>
            new WavePhaseDefinition(id, id, tag, duration, interval, maxAlive, composition, modifiers);

        // Ordinary (0-10s) -> Pressure (10-25s, faster/frailer) -> Rest (25-30s).
        public static WaveTimelineDefinition ThreePhaseTimeline(int seed = 7) =>
            new WaveTimelineDefinition(
                "FIXTURE-WAVE-TEST",
                seed,
                SpawnRadius,
                new[]
                {
                    Phase("FIXTURE-PHASE-ORDINARY", WavePhaseTag.Ordinary, 10f, 2f, 4, null, Entry("FIXTURE-ENEMY-A")),
                    Phase("FIXTURE-PHASE-PRESSURE", WavePhaseTag.Pressure, 15f, 0.5f, 8,
                        new WaveEnemyModifiers(healthMultiplier: 0.5f, speedMultiplier: 1.5f),
                        Entry("FIXTURE-ENEMY-B")),
                    Phase("FIXTURE-PHASE-REST", WavePhaseTag.Rest, 5f, 4f, 2, null, Entry("FIXTURE-ENEMY-A"))
                },
                new[]
                {
                    new WaveHookDefinition(WaveHookKind.MidBoss, 12f),
                    new WaveHookDefinition(WaveHookKind.FinalBoss, 25f)
                });

        public static Dictionary<ContentId, EnemyDefinition> TestEnemies() =>
            Enemies(Enemy("FIXTURE-ENEMY-A"), Enemy("FIXTURE-ENEMY-B", health: 20f, speed: 2f));
    }
}

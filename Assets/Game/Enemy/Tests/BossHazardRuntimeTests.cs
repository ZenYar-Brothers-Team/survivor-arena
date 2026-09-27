using System.Linq;
using System.Reflection;
using Game.Character;
using Game.Content;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Enemy.Tests
{
    /// <summary>DECISION-0066 runtime: hazards hit the real player, boss death clears them, summons outlive the boss and end with the run.</summary>
    public sealed class BossHazardRuntimeTests
    {
        private static readonly MethodInfo FixedUpdate =
            typeof(BossEncounterRuntime).GetMethod("FixedUpdate", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly ContentId Source = new ContentId("FIXTURE-BOSS-STEP");
        private GameObject _root;
        private RunController _run;
        private PlayerCharacterRuntime _player;
        private BossEncounterRuntime _bosses;
        private WaveDirector _director;
        private EnemyDefinition _minion;
        private int _baseline;

        [SetUp]
        public void SetUp()
        {
            _baseline = EnemyRegistry.Count;
            _root = new GameObject("boss hazard tests");
            _run = _root.AddComponent<RunController>();
            _run.Initialize();
            _run.Model.Start();
            var target = new GameObject("target");
            target.transform.SetParent(_root.transform);
            _player = target.AddComponent<PlayerCharacterRuntime>();
            _player.Initialize(new CharacterBaseStats(100, 0), _run);
            _bosses = _root.AddComponent<BossEncounterRuntime>();
            var enemies = FixtureEnemyCatalog.Create();
            _minion = enemies[0];
            _director = new WaveDirector(FixtureWaveTimelineCatalog.Create(), enemies.ToDictionary(e => e.Id), _run.Model.Duration);
            _bosses.Initialize(_director, _run, _player.transform, FixtureBossCatalog.Create(enemies));
        }

        [TearDown]
        public void TearDown()
        {
            _bosses.Shutdown();
            Object.DestroyImmediate(_root);
            Assert.AreEqual(_baseline, EnemyRegistry.Count);
        }

        private EnemyRuntime SpawnFinal()
        {
            var time = _director.Timeline.Hooks.Single(h => h.Kind == WaveHookKind.FinalBoss).TimeSeconds;
            _director.Advance(time, time, true, 10000);
            return _bosses.FinalBoss;
        }

        private void Step(float seconds)
        {
            for (var t = 0f; t < seconds; t += Time.fixedDeltaTime) FixedUpdate.Invoke(_bosses, null);
        }

        [Test]
        public void Zone_HitsThePlayerStandingInIt_AfterTheFill()
        {
            var boss = SpawnFinal();
            var zone = BossHazardTestData.Zone(BossZonePlacement.AtPlayer, radius: 2f, fill: 1f, damage: 26f);
            _bosses.HazardsOf(boss).Start(BossSpecialRequest.ForZone(zone, Source), boss.Position, _player.transform.position);
            Step(0.9f);
            Assert.AreEqual(100f, _player.Health.CurrentHealth, 1e-3f, "No damage while filling.");
            Step(0.2f);
            Assert.AreEqual(74f, _player.Health.CurrentHealth, 1e-3f);
        }

        [Test]
        public void Zone_PausedRunNeverFills_AndBossDeathClearsIt()
        {
            var boss = SpawnFinal();
            var zone = BossHazardTestData.Zone(BossZonePlacement.AtPlayer, radius: 2f, fill: 1f, damage: 26f);
            var field = _bosses.HazardsOf(boss);
            field.Start(BossSpecialRequest.ForZone(zone, Source), boss.Position, _player.transform.position);
            _run.TogglePause();
            Step(3f);
            _run.TogglePause();
            Assert.AreEqual(100f, _player.Health.CurrentHealth, 1e-3f);
            boss.TakeDamage(100000f);
            Assert.IsTrue(field.IsEmpty, "Boss death removes its pending zones.");
            Step(2f);
            Assert.AreEqual(100f, _player.Health.CurrentHealth, 1e-3f);
        }

        [Test]
        public void Summon_SpawnsOrdinaryEnemies_ThatOutliveTheBossAndEndWithTheRun()
        {
            var boss = SpawnFinal();
            var bossesAlive = EnemyRegistry.Count;
            var summon = new BossSummonProfile(_minion, 3, 6f, 4, 0.8f, BossHazardTestData.Color);
            _bosses.HazardsOf(boss).Start(BossSpecialRequest.ForSummon(summon, Source), boss.Position, _player.transform.position);
            Step(1f);
            Assert.AreEqual(3, _bosses.SummonedAlive);
            Assert.AreEqual(bossesAlive + 3, EnemyRegistry.Count);
            var alive = new System.Collections.Generic.List<EnemyRuntime>();
            EnemyRegistry.CopyAliveTo(alive);
            var summoned = alive.Where(e => e.ContentId == _minion.Id).ToList();
            Assert.AreEqual(3, summoned.Count);
            Assert.IsTrue(summoned.All(e => e.Category == EnemyCategory.Ordinary), "Summons are ordinary enemies.");
            boss.Despawn();
            Assert.AreEqual(3, _bosses.SummonedAlive, "Summons outlive their boss.");
            _run.Model.Tick(_run.Model.Duration);
            Assert.AreEqual(RunState.Won, _run.Model.State);
            Assert.AreEqual(0, _bosses.SummonedAlive, "Run end cleans the summons.");
            Assert.AreEqual(_baseline, EnemyRegistry.Count);
        }
    }
}

using System;
using System.Linq;
using Game.Character;
using Game.Combat;
using Game.Run;
using NUnit.Framework;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Game.Enemy.Tests
{
    public sealed class BossEncounterTests
    {
        private GameObject _root;
        private RunController _run;
        private PlayerCharacterRuntime _player;
        private BossEncounterRuntime _bosses;
        private WaveDirector _director;
        private int _baseline;

        [SetUp]
        public void SetUp()
        {
            _baseline = EnemyRegistry.Count;
            _root = new GameObject("boss tests");
            _run = _root.AddComponent<RunController>();
            _run.Initialize();
            _run.Model.Start();
            var target = new GameObject("target");
            target.transform.SetParent(_root.transform);
            _player = target.AddComponent<PlayerCharacterRuntime>();
            _player.Initialize(new CharacterBaseStats(100, 0), _run);
            _bosses = _root.AddComponent<BossEncounterRuntime>();
            Initialize();
        }

        private void Initialize()
        {
            var enemies = FixtureEnemyCatalog.Create();
            _director = new WaveDirector(FixtureWaveTimelineCatalog.Create(), enemies.ToDictionary(e => e.Id), _run.Model.Duration);
            _bosses.Initialize(_director, _run, _player.transform, FixtureBossCatalog.Create(enemies));
        }

        private EnemyRuntime SpawnFinal()
        {
            var time = _director.Timeline.Hooks.Single(h => h.Kind == WaveHookKind.FinalBoss).TimeSeconds;
            _director.Advance(time, time, true, 10000);
            return _bosses.FinalBoss;
        }

        [TearDown]
        public void TearDown()
        {
            _bosses.Shutdown();
            Object.DestroyImmediate(_root);
            Assert.AreEqual(_baseline, EnemyRegistry.Count);
        }

        [Test]
        public void Hooks_FullRegularCapAndRepeatedAdvance_SpawnEachBossOnce()
        {
            var spawned = 0;
            _bosses.LifeEvent += e => { if (e.Kind == EnemyLifeEventKind.Spawned) spawned++; };
            var final = SpawnFinal();
            Assert.IsNotNull(final);
            Assert.AreEqual(EnemyCategory.Boss, final.Category);
            Assert.AreEqual(2, spawned);
            _director.Advance(_director.Elapsed, 0, true, 10000);
            Assert.AreEqual(2, spawned);
            Assert.AreEqual(_baseline + 2, EnemyRegistry.Count);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void Timer_BossAliveOrKilled_WinsAndCleansAllLives(bool killed)
        {
            var final = SpawnFinal();
            if (killed) final.TakeDamage(10000);
            Assert.AreEqual(RunState.Running, _run.Model.State);
            _run.Model.Tick(_run.Model.Duration);
            Assert.AreEqual(RunState.Won, _run.Model.State);
            Assert.IsNull(_bosses.FinalBoss);
            Assert.AreEqual(_baseline, EnemyRegistry.Count);
        }

        [Test]
        public void Lifecycle_DeathDespawnAndPlayerDeath_AreDistinctAndCleanupDoesNotDependOnTelemetry()
        {
            var died = 0;
            var cleanup = 0;
            _bosses.LifeEvent += e =>
            {
                if (e.Kind == EnemyLifeEventKind.Died) died++;
                if (e.Kind == EnemyLifeEventKind.Despawned && e.Reason == EnemyLifeReason.Cleanup) cleanup++;
            };
            var final = SpawnFinal();
            final.Despawn();
            Assert.IsNull(_bosses.FinalBoss);
            Assert.AreEqual(0, died);
            Assert.AreEqual(1, cleanup);
            _player.ApplyDamage(new CombatDamageRequest(default, 10000));
            Assert.AreEqual(RunState.Lost, _run.Model.State);
            Assert.AreEqual(_baseline, EnemyRegistry.Count);
            Assert.AreEqual(2, cleanup);
        }

        [Test]
        public void Shutdown_Reinitialize_IgnoresOldDirectorAndResetsLifeAndPhase()
        {
            var oldDirector = _director;
            _bosses.Shutdown();
            Initialize();
            oldDirector.Advance(899, 899, true, 0);
            Assert.IsNull(_bosses.FinalBoss);
            var first = SpawnFinal();
            var firstId = first.LifeId;
            first.BossCombat.Tick(0, true, .1f, Vector2.right);
            _bosses.Shutdown();
            Initialize();
            var second = SpawnFinal();
            Assert.AreNotEqual(firstId, second.LifeId);
            Assert.AreEqual(0, second.BossCombat.PhaseIndex);
            Assert.AreEqual(second.Health.MaxHealth, second.Health.CurrentHealth);
        }

        [Test]
        public void Hook_PausedOrEnded_DoesNotCreateBoss()
        {
            _run.TogglePause();
            _director.Advance(899, 899, false, 0);
            Assert.IsNull(_bosses.FinalBoss);
            _run.Model.Stop();
            _director.Advance(899, 899, true, 0);
            Assert.IsNull(_bosses.FinalBoss);
        }

        [Test]
        public void Pause_SuspendsBossPhysicsAndResumeRestoresItIncludingPoolReuse()
        {
            var boss = SpawnFinal();
            _run.TogglePause();
            Assert.IsFalse(boss.GetComponent<Rigidbody2D>().simulated);
            _run.TogglePause();
            Assert.IsTrue(boss.GetComponent<Rigidbody2D>().simulated);
            _run.TogglePause();
            _bosses.Shutdown();
            _run.TogglePause();
            Initialize();
            Assert.IsTrue(SpawnFinal().GetComponent<Rigidbody2D>().simulated);
        }
    }
}

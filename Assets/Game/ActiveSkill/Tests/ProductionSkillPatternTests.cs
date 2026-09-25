using System.Collections.Generic;
using System.Linq;
using Game.Content;
using Game.Enemy;
using Game.Presentation;
using Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    /// <summary>F1-01 executor contracts: continuous orbit, expanding wave and deferred telegraphed strikes.</summary>
    public sealed class ProductionSkillPatternTests
    {
        private GameObject _runObject;
        private GameObject _owner;
        private RunController _runController;
        private SceneActiveSkillEffectExecutor _executor;
        private readonly List<EnemyRuntime> _enemies = new List<EnemyRuntime>();

        [SetUp]
        public void SetUp()
        {
            _runObject = new GameObject("RunController");
            _runController = _runObject.AddComponent<RunController>();
            TestLifecycle.InvokeAwake(_runController);
            _runController.Model.Start();
            _owner = new GameObject("Owner");
        }

        [TearDown]
        public void TearDown()
        {
            _executor?.Dispose();
            foreach (var enemy in _enemies)
                if (enemy != null) enemy.Despawn();
            _enemies.Clear();
            Object.DestroyImmediate(_owner);
            Object.DestroyImmediate(_runObject);
        }

        [Test]
        public void PersistentOrbit_SweepsAcrossTargets_HitsOncePerBladeCooldown_AndSurvivesUpgrade()
        {
            var enemy = SpawnEnemy(new Vector2(0f, 1f), 100f);
            _executor = new SceneActiveSkillEffectExecutor(_runController);
            var orbit = new OrbitEffect(1, 1f, 180f, 0f, 0.6f, bladeHitboxRadius: 0.1f, persistent: true);
            _executor.Schedule(Activation(Level(orbit, 0.2f), null, 5f));
            Assert.AreEqual(1, _executor.PersistentOrbitCount);

            // One large step sweeps the blade from 0° past 90° without sampling that angle exactly.
            _executor.Tick(0.9f, true);
            Assert.AreEqual(95f, enemy.Health.CurrentHealth, 1e-4f, "Swept blade path must hit a target it passes.");
            _executor.Tick(0.1f, true);
            Assert.AreEqual(95f, enemy.Health.CurrentHealth, 1e-4f);

            _executor.TryGetPersistentOrbit("FIXTURE-SKILL-EXECUTOR", out _, out var phaseBefore);
            var upgraded = new OrbitEffect(3, 1f, 180f, 0f, 0.6f, bladeHitboxRadius: 0.1f, persistent: true);
            _executor.Schedule(Activation(Level(upgraded, 0.2f), null, 5f));
            Assert.AreEqual(1, _executor.PersistentOrbitCount, "Refresh never duplicates the orbit.");
            _executor.TryGetPersistentOrbit("FIXTURE-SKILL-EXECUTOR", out var blades, out var phaseAfter);
            Assert.AreEqual(3, blades);
            Assert.AreEqual(phaseBefore, phaseAfter, 1e-4f, "Upgrade keeps the running phase.");
        }

        [Test]
        public void PersistentOrbit_PausesAndExpiresWithoutRefresh()
        {
            _executor = new SceneActiveSkillEffectExecutor(_runController);
            var orbit = new OrbitEffect(2, 1f, 90f, 0f, 0.6f, bladeHitboxRadius: 0.1f, persistent: true);
            _executor.Schedule(Activation(Level(orbit, 0.2f), null, 5f));
            _executor.TryGetPersistentOrbit("FIXTURE-SKILL-EXECUTOR", out _, out var start);
            _executor.Tick(10f, false);
            _executor.TryGetPersistentOrbit("FIXTURE-SKILL-EXECUTOR", out _, out var paused);
            Assert.AreEqual(start, paused);
            _executor.Tick(SceneActiveSkillEffectExecutor.PersistentOrbitMinimumLeaseSeconds + 0.1f, true);
            Assert.AreEqual(0, _executor.PersistentOrbitCount);
        }

        [Test]
        public void ExpandingArea_DamagesOnlyWhenFrontArrives_OncePerWave()
        {
            var near = SpawnEnemy(new Vector2(0.5f, 0f), 100f);
            var far = SpawnEnemy(new Vector2(2f, 0f), 100f);
            _executor = new SceneActiveSkillEffectExecutor(_runController);
            var level = new ActiveSkillLevelDefinition(1f, 4f, ActiveSkillTargetingMode.Self,
                new ActiveSkillActivationWave(0f, 0f, 1f, new AreaEffect(2.5f, 1f, 0.25f)));
            _executor.Schedule(Activation(level, null, 8f));

            _executor.Tick(0.05f, true); // front 0.5 + enemy collider radius reaches only the near target
            Assert.AreEqual(92f, near.Health.CurrentHealth, 1e-4f);
            Assert.AreEqual(100f, far.Health.CurrentHealth, 1e-4f);
            _executor.Tick(0.3f, true);
            Assert.AreEqual(92f, near.Health.CurrentHealth, 1e-4f, "Hit once per wave.");
            Assert.AreEqual(92f, far.Health.CurrentHealth, 1e-4f);
            Assert.AreEqual(0, _executor.ExpandingAreaCount);
        }

        [Test]
        public void Strikes_SnapshotLaterTargetsAtTheirOwnTelegraphStart_AndSkipWithoutTarget()
        {
            var first = SpawnEnemy(new Vector2(3f, 0f), 200f);
            _executor = new SceneActiveSkillEffectExecutor(_runController);
            var level = new ActiveSkillLevelDefinition(1f, 4.5f,
                new ActiveSkillTargetingProfile(ActiveSkillTargetingMode.RandomEnemy, 8f, 7), default,
                new ActiveSkillActivationWave(0f, 0f, 1f, new StrikeEffect(0.5f, 0.6f)),
                new ActiveSkillActivationWave(0.3f, 0f, 1f, new StrikeEffect(0.5f, 0.6f)),
                new ActiveSkillActivationWave(0.6f, 0f, 1f, new StrikeEffect(0.5f, 0.6f)));
            _executor.Schedule(Activation(level, first, 10f, random: new System.Random(7)));

            // A second enemy appears after activation but before the second telegraph starts.
            _executor.Tick(0.1f, true);
            var late = SpawnEnemy(new Vector2(-3f, 0f), 200f);
            _executor.Tick(0.25f, true); // t=0.35: second strike telegraph started on the late enemy
            first.transform.position = new Vector3(6f, 6f, 0f); // leaving after snapshot does not move the strike
            Physics2D.SyncTransforms();
            _executor.Tick(0.3f, true); // t=0.65: first strike lands on the old point
            Assert.AreEqual(200f, first.Health.CurrentHealth, 1e-4f);
            _executor.Tick(0.35f, true); // t=1.0: second strike lands
            Assert.AreEqual(190f, late.Health.CurrentHealth, 1e-4f);
            _executor.Tick(1f, true); // third strike found no unused target and was skipped
            Assert.AreEqual(190f, late.Health.CurrentHealth, 1e-4f);
            Assert.AreEqual(0, _executor.PendingStrikeCount);
        }

        [Test]
        public void WorldEffects_AreReturnedOnTerminalClear()
        {
            SpawnEnemy(new Vector2(1f, 0f), 100f);
            var profiles = SkillWorldEffectCatalog.FromJson(
                "[{\"skillId\":\"FIXTURE-SKILL-EXECUTOR\",\"kind\":\"ExpandingRing\",\"color\":[1,1,1,1],\"impactColor\":[1,1,1,1],\"thickness\":0.1,\"fadeSeconds\":0.1}]");
            _executor = new SceneActiveSkillEffectExecutor(_runController, worldEffectProfiles: profiles);
            var level = new ActiveSkillLevelDefinition(1f, 4f, ActiveSkillTargetingMode.Self,
                new ActiveSkillActivationWave(0f, 0f, 1f, new AreaEffect(2f, 1f, 0.25f)));
            _executor.Schedule(Activation(level, null, 1f));
            _executor.Tick(0.1f, true);
            Assert.AreEqual(1, _executor.ActiveWorldEffectShapeCount);
            _executor.Clear();
            Assert.AreEqual(0, _executor.ActiveWorldEffectShapeCount);
            Assert.AreEqual(0, _executor.ExpandingAreaCount);
        }

        [Test]
        public void StrikePillar_AppearsBeforeTheImpact_AndGroundDiscIsFlattened()
        {
            var profiles = SkillWorldEffectCatalog.FromJson(
                "[{\"skillId\":\"FIXTURE-SKILL-EXECUTOR\",\"kind\":\"StrikeTelegraph\",\"color\":[1,1,1,1],\"impactColor\":[1,1,1,1]," +
                "\"thickness\":0.1,\"fadeSeconds\":0.3,\"pillarWidth\":0.7,\"pillarHeight\":3,\"pillarLeadSeconds\":0.15}]");
            _executor = new SceneActiveSkillEffectExecutor(_runController, worldEffectProfiles: profiles);
            var level = new ActiveSkillLevelDefinition(1f, 4f, ActiveSkillTargetingMode.Self,
                new ActiveSkillActivationWave(0f, 0f, 1f, new StrikeEffect(0.5f, 0.6f, verticalScale: 0.7f)));
            _executor.Schedule(Activation(level, null, 1f));
            _executor.Tick(0f, true);
            Assert.AreEqual(1, _executor.ActiveWorldEffectShapeCount, "Only the telegraph disc at first.");
            var disc = Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None)
                .Single(renderer => renderer.enabled && renderer.name == "SkillWorldEffect");
            Assert.AreEqual(0.7f, disc.transform.localScale.y / disc.transform.localScale.x, 1e-4f,
                "DECISION-0058: the ground disc is seen at the 3/4 camera angle.");

            _executor.Tick(0.4f, true); // 0.2 s left: pillar not yet
            Assert.AreEqual(1, _executor.ActiveWorldEffectShapeCount);
            _executor.Tick(0.1f, true); // 0.1 s left: inside the 0.15 s lead
            Assert.AreEqual(2, _executor.ActiveWorldEffectShapeCount, "DECISION-0058: pillar lands before the flash.");
            _executor.Tick(0.1f, true); // impact: pillar reused, not duplicated
            Assert.AreEqual(2, _executor.ActiveWorldEffectShapeCount, "Flash and the same pillar; the telegraph is gone.");
            _executor.Clear();
        }

        private static ActiveSkillLevelDefinition Level(OrbitEffect orbit, float refresh) =>
            new ActiveSkillLevelDefinition(1f, refresh, ActiveSkillTargetingMode.Self,
                new ActiveSkillActivationWave(0f, 0f, 1f, orbit));

        private EnemyRuntime SpawnEnemy(Vector2 position, float health)
        {
            var definition = new EnemyDefinition($"FIXTURE-ENEMY-P{_enemies.Count}", health, 0.2f, 0f, 0f, 1f);
            var enemy = EnemyFactory.Spawn(definition, position, _owner.transform, _runController);
            _enemies.Add(enemy);
            Physics2D.SyncTransforms();
            return enemy;
        }

        private ActiveSkillActivation Activation(ActiveSkillLevelDefinition level, IEnemyDamageReceiver target,
            float damage, System.Random random = null)
        {
            var direction = target != null ? target.Position : Vector2.right;
            return new ActiveSkillActivation("FIXTURE-SKILL-EXECUTOR", 1, Vector2.zero, direction, target, damage,
                level, _owner.transform, random: random);
        }
    }
}

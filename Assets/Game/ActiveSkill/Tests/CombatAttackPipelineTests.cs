using Game.Character;
using Game.Combat;
using Game.Content;
using Game.Enemy;
using Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    public sealed class CombatAttackPipelineTests
    {
        private GameObject _root;
        private RunController _run;
        private PlayerCharacterRuntime _owner;
        private EnemyRuntime _target;
        private SceneActiveSkillEffectExecutor _executor;
        private CombatRecordingLauncher _launcher;
        private CombatResult _lastHit;

        [SetUp]
        public void SetUp()
        {
            _root = new GameObject("combat-pipeline-test");
            _run = _root.AddComponent<RunController>();
            if (!_run.IsInitialized) _run.Initialize();
            _run.Model.Start();
            var player = new GameObject("player");
            player.transform.SetParent(_root.transform);
            _owner = player.AddComponent<PlayerCharacterRuntime>();
            _owner.Initialize(new CharacterBaseStats(100f, 3f), _run, "FIXTURE-OWNER");
            _target = EnemyFactory.Spawn(new EnemyDefinition("FIXTURE-TARGET", 100f, 0.2f, 0f, 0f, 1f),
                Vector2.right * 1.1f, player.transform, _run, _root.transform);
            _target.CombatResolved += result => _lastHit = result;
            _launcher = new CombatRecordingLauncher();
            _executor = new SceneActiveSkillEffectExecutor(_run, _launcher);
            Physics2D.SyncTransforms();
        }

        [TearDown]
        public void TearDown()
        {
            _executor.Dispose();
            Object.DestroyImmediate(_root);
        }

        [TestCase("projectile")]
        [TestCase("boomerang")]
        [TestCase("beam")]
        [TestCase("orbit")]
        [TestCase("chain")]
        [TestCase("area")]
        [TestCase("mine")]
        public void EveryEffectFamily_PreservesDamageSourceLevelAndControls(string kind)
        {
            IActiveSkillEffect effect = kind switch
            {
                "projectile" => new ProjectileBurstEffect(1, ProjectileLayout.Fan, 0f, 0, 10f, 2f, 0.1f),
                "boomerang" => new BoomerangEffect(1, 0f, 10f, 5f, 0.1f, 0.5f),
                "beam" => new BeamEffect(0.1f, 0.1f, 0.5f, 3f, true),
                "orbit" => new OrbitEffect(1, 1f, 90f, 0.1f, 0.1f),
                "chain" => new ChainEffect(1, 2f, 1f),
                "area" => new AreaEffect(2f),
                _ => new MineEffect(2f, 2f, 1f, 1)
            };
            var controls = new CombatControlProfile(1f, 0.15f, 0.2f, 1f);
            var level = new ActiveSkillLevelDefinition(4f, 1f, ActiveSkillTargetingMode.Self,
                new ActiveSkillActivationWave(0f, 0f, 1f, controls, effect));
            _executor.Schedule(new ActiveSkillActivation("FIXTURE-SKILL-ALL", 3, Vector2.zero, Vector2.right,
                _target, 4f, level, _owner.transform, _owner.Identity, 1.25f));
            _executor.Tick(0f, true);
            FixtureProjectileRuntime projectile = null;
            if (_launcher.Projectiles.Count > 0)
            {
                projectile = FixtureProjectileFactory.Spawn(_launcher.Projectiles[0], _run, _root.transform);
                Assert.IsTrue(projectile.TryImpact(_target, _target.Position));
            }
            Assert.AreEqual(4f, _lastHit.Health.Actual);
            Assert.AreEqual(_owner.Identity.LifeId, _lastHit.Source.Owner.LifeId);
            Assert.AreEqual(new ContentId("FIXTURE-SKILL-ALL"), _lastHit.Source.ContentId);
            Assert.AreEqual(CombatSourceOrigin.ActiveSkill, _lastHit.Source.Origin);
            Assert.AreEqual(3, _lastHit.Source.SkillLevel);
            Assert.AreEqual(0.8f, _target.Controls.MovementMultiplier, 0.0001f);
            Assert.AreEqual(1.25f, _lastHit.ResolvedKnockbackDistance, 0.0001f);
            if (kind == "boomerang")
            {
                projectile.Simulate(0.2f);
                projectile.Simulate(0.2f);
                projectile.Simulate(0.1f);
                Assert.IsTrue(projectile.TryImpact(_target, _target.Position));
                Assert.AreEqual(2f, _lastHit.Health.Actual);
                Assert.AreEqual(3, _lastHit.Source.SkillLevel);
                Assert.AreEqual(_owner.Identity.LifeId, _lastHit.Source.Owner.LifeId);
            }
        }

        [Test]
        public void LowHpDamage_IsCapturedOnceAtActivation_DelayedHitSurvivesHealAndOwnerShutdown()
        {
            _owner.SetModifier("low-hp", new CharacterStatModifier(lowHealthDamageMaxBonus: 0.9f));
            _owner.TakeDamage(50f);
            var level = new ActiveSkillLevelDefinition(10f, 1f, ActiveSkillTargetingMode.Self,
                new ActiveSkillActivationWave(0.5f, 0f, 1f, new AreaEffect(2f)));
            var definition = new ActiveSkillProgressionDefinition("FIXTURE-SNAPSHOT", "Snapshot", level, level, level, level, level, level);
            var skill = new ActiveSkillInstance(definition);
            var targets = new SceneEnemyTargetProvider();
            var identity = _owner.Identity;
            skill.Tick(0f, true, _owner, targets, _executor);
            _owner.Heal(100f);
            _executor.Tick(0.5f, true);
            Assert.AreEqual(15f, _lastHit.Health.Actual, 0.0001f);
            skill.Tick(1f, true, _owner, targets, _executor);
            _owner.Shutdown();
            _executor.Tick(0.5f, true);
            Assert.AreEqual(10f, _lastHit.Health.Actual);
            Assert.AreEqual(identity.LifeId, _lastHit.Source.Owner.LifeId);
            Assert.AreEqual(identity.ContentId, _lastHit.Source.Owner.ContentId);
        }
    }
}

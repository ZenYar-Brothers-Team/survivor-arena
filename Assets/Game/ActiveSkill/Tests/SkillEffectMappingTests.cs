using System.Linq;
using Game.Enemy;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    public sealed class SkillEffectMappingTests
    {
        [TestCase("beam")]
        [TestCase("orbit")]
        [TestCase("area")]
        public void SpatialMapping_SeparatesAttackSizeFromReachInRealDamageQueries(string kind)
        {
            using var context = new SkillFrameworkTestContext();
            var hit = context.Enemy(kind == "beam" ? new Vector2(5f, .4f) : kind == "orbit" ? new Vector2(3f, 0f) : new Vector2(1.8f, 0f));
            var miss = context.Enemy(kind == "beam" ? new Vector2(5f, .6f) : kind == "orbit" ? new Vector2(2f, 0f) : new Vector2(3f, 0f));
            IActiveSkillEffect effect = kind == "beam" ? new BeamEffect(.1f, .1f, .5f, 2f, false) :
                kind == "orbit" ? new OrbitEffect(1, 1f, 90f, .1f, .1f, bladeHitboxRadius: .1f) : new AreaEffect(1f);
            var level = new ActiveSkillLevelDefinition(10, 1, ActiveSkillTargetingMode.Self, new ActiveSkillActivationWave(0, 0, 1, effect));
            using var executor = new SceneActiveSkillEffectExecutor(context.Run, new CombatRecordingLauncher());
            executor.Schedule(new ActiveSkillActivation("FIXTURE-SPATIAL", 1, Vector2.zero, Vector2.right, null,
                10f, level, context.Owner.transform, sizeMultiplier: 2f, rangeMultiplier: 3f));
            Physics2D.SyncTransforms();
            executor.Tick(0, true);
            Assert.AreEqual(90f, hit.Health.CurrentHealth);
            Assert.AreEqual(100f, miss.Health.CurrentHealth);
        }

        [Test]
        public void DelayedRandomArea_UsesCapturedPointAfterTargetMoves()
        {
            using var context = new SkillFrameworkTestContext();
            var chosen = context.Enemy(new Vector2(20f, 0f));
            var definition = FixtureActiveSkillCatalog.Create().Single(s => s.Id.ToString() == "FIXTURE-SKILL-WORLD-TARGET");
            using var executor = new SceneActiveSkillEffectExecutor(context.Run, new CombatRecordingLauncher());
            new ActiveSkillInstance(definition).Tick(0, true, context.Player, new SceneEnemyTargetProvider(), executor);
            chosen.transform.position = new Vector2(40f, 0f);
            var replacement = context.Enemy(new Vector2(20f, 0f));
            Physics2D.SyncTransforms();
            executor.Tick(.5f, true);
            Assert.AreEqual(100f, chosen.Health.CurrentHealth);
            Assert.AreEqual(80f, replacement.Health.CurrentHealth);
        }

        [Test]
        public void ProjectileMapping_ScalesHitSizeAndLifetimeOnceWithoutScalingSpeed()
        {
            using var context = new SkillFrameworkTestContext();
            var launcher = new CombatRecordingLauncher();
            using var executor = new SceneActiveSkillEffectExecutor(context.Run, launcher);
            var effect = new ProjectileBurstEffect(1, ProjectileLayout.Single, 0f, 2, 8f, 2f, .2f, .8f);
            var level = new ActiveSkillLevelDefinition(10f, 1f, ActiveSkillTargetingMode.Self,
                new ActiveSkillActivationWave(.5f, 0f, 1f, effect));
            executor.Schedule(new ActiveSkillActivation("FIXTURE-MAPPING", 6, Vector2.zero, Vector2.right, null,
                10f, level, context.Owner.transform, sizeMultiplier: 2f, rangeMultiplier: 3f));
            executor.Tick(.49f, true);
            Assert.AreEqual(0, launcher.Projectiles.Count);
            executor.Tick(.02f, true);
            var shot = launcher.Projectiles.Single();
            Assert.AreEqual(8f, shot.Speed);
            Assert.AreEqual(6f, shot.LifetimeSeconds);
            Assert.AreEqual(.4f, shot.CollisionRadius);
            Assert.AreEqual(1.6f, shot.ImpactAreaRadius);
            Assert.AreEqual(10f, shot.Damage.Amount);
        }

        [Test]
        public void Chain_UsesLastHitAsOriginAndNeverRepeatsTargets()
        {
            using var context = new SkillFrameworkTestContext();
            var first = context.Enemy(new Vector2(1f, 0));
            var second = context.Enemy(new Vector2(2f, 0));
            var third = context.Enemy(new Vector2(3f, 0));
            using var executor = new SceneActiveSkillEffectExecutor(context.Run, new CombatRecordingLauncher());
            var level = new ActiveSkillLevelDefinition(10f, 1f, ActiveSkillTargetingMode.NearestEnemy,
                new ActiveSkillActivationWave(0, 0, 1, new ChainEffect(8, .55f, .5f)));
            executor.Schedule(new ActiveSkillActivation("FIXTURE-CHAIN", 1, Vector2.zero, Vector2.right, first,
                10f, level, context.Owner.transform, rangeMultiplier: 2f));
            executor.Tick(0, true);
            Assert.AreEqual(90f, first.Health.CurrentHealth);
            Assert.AreEqual(95f, second.Health.CurrentHealth);
            Assert.AreEqual(97.5f, third.Health.CurrentHealth, .0001f, "Third is out of first-target range but inside second-target range.");
        }

        [Test]
        public void RandomDelayedWaves_UseDistinctWorldTargetsAndFreezeTheirPositions()
        {
            using var context = new SkillFrameworkTestContext();
            var first = context.Enemy(new Vector2(20, 0));
            var second = context.Enemy(new Vector2(30, 0));
            var definition = FixtureActiveSkillCatalog.Create().Single(s => s.Id.ToString() == "FIXTURE-SKILL-WORLD-TARGET");
            using var executor = new SceneActiveSkillEffectExecutor(context.Run, new CombatRecordingLauncher());
            var instance = new ActiveSkillInstance(definition);
            instance.SetLevel(4);
            instance.Tick(0, true, context.Player, new SceneEnemyTargetProvider(), executor);
            Physics2D.SyncTransforms();
            executor.Tick(.5f, true);
            Assert.AreEqual(180f, first.Health.CurrentHealth + second.Health.CurrentHealth);
            executor.Tick(.25f, true);
            Assert.AreEqual(80f, first.Health.CurrentHealth);
            Assert.AreEqual(80f, second.Health.CurrentHealth);
        }

        [Test]
        public void IndependentDirections_AreSeededAndNotOneRotatedFan()
        {
            var a = ProjectileDirectionGenerator.Create(ProjectileLayout.IndependentRandom, 4, Vector2.right, random: new System.Random(41));
            var b = ProjectileDirectionGenerator.Create(ProjectileLayout.IndependentRandom, 4, Vector2.up, random: new System.Random(41));
            CollectionAssert.AreEqual(a, b);
            Assert.AreNotEqual(a[0], a[1]);
            Assert.AreNotEqual(Vector2.Angle(a[0], a[1]), Vector2.Angle(a[1], a[2]));
        }

        [Test]
        public void UpgradePreview_IncludesChangedResolvedSpatialParameters()
        {
            var skill = FixtureActiveSkillCatalog.Create().Single(s => s.Id.ToString() == "FIXTURE-SKILL-MOVEMENT");
            var preview = skill.CreateDraftPreview(3, 4);
            var radius = preview.Values.Single(v => v.Label.Contains("projectile radius"));
            Assert.AreEqual(.15f, radius.Current);
            Assert.AreEqual(.2025f, radius.Next, .0001f);
        }
    }
}

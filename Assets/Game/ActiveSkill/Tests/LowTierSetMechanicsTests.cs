using System.Linq;
using Game.Combat;
using Game.Content;
using Game.Progression;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    /// <summary>Low-tier sets (DECISION-0138): extra projectiles, slow strength, and the SET-022 cone.</summary>
    public sealed class LowTierSetMechanicsTests
    {
        private SkillFrameworkTestContext _context;
        private CombatRecordingLauncher _launcher;
        private SceneActiveSkillEffectExecutor _executor;

        [SetUp]
        public void SetUp()
        {
            _context = new SkillFrameworkTestContext();
            _launcher = new CombatRecordingLauncher();
            _executor = new SceneActiveSkillEffectExecutor(_context.Run, _launcher);
        }

        [TearDown]
        public void TearDown()
        {
            _executor.Dispose();
            _context.Dispose();
        }

        [Test]
        public void ExtraProjectiles_AddToTheBurst()
        {
            Fire(Burst(new ProjectileBurstEffect(4, ProjectileLayout.Fan, 60f, 0, 8f, 1f, 0.2f)), new SkillMechanicBonus(extraProjectiles: 2));
            Assert.AreEqual(6, _launcher.Projectiles.Count);
        }

        [Test]
        public void SlowStrengthBonus_StrengthensAnExistingSlowOnly()
        {
            var slow = new CombatControlProfile(slowFraction: 0.2f, slowSeconds: 1.5f);
            Fire(Burst(new ProjectileBurstEffect(1, ProjectileLayout.Single, 0f, 0, 8f, 1f, 0.2f), slow), new SkillMechanicBonus(slowStrengthBonus: 0.05f));
            Assert.AreEqual(0.25f, _launcher.Projectiles[0].Damage.Combat.Controls.SlowFraction, 1e-5f);
            Assert.AreEqual(1.5f, _launcher.Projectiles[0].Damage.Combat.Controls.SlowSeconds, 1e-5f);

            Fire(Burst(new ProjectileBurstEffect(1, ProjectileLayout.Single, 0f, 0, 8f, 1f, 0.2f)), new SkillMechanicBonus(slowStrengthBonus: 0.05f));
            Assert.AreEqual(0f, _launcher.Projectiles[1].Damage.Combat.Controls.SlowFraction, "A bonus never creates a slow.");
        }

        [Test]
        public void GrantedSlow_AddsASlowToASkillWithoutOne_KeepingItsKnockback_AndTheStrongerSlowWins()
        {
            var push = new CombatControlProfile(2.4f, 0.12f);
            Fire(Burst(new ProjectileBurstEffect(1, ProjectileLayout.Single, 0f, 0, 8f, 1f, 0.2f), push),
                new SkillMechanicBonus(grantedSlowFraction: 0.3f, grantedSlowSeconds: 1.5f));
            var granted = _launcher.Projectiles[0].Damage.Combat.Controls;
            Assert.AreEqual(0.3f, granted.SlowFraction, 1e-5f, "SET-004 (DECISION-0139): the wave slows by itself.");
            Assert.AreEqual(1.5f, granted.SlowSeconds, 1e-5f);
            Assert.AreEqual(2.4f, granted.KnockbackDistance, 1e-5f);

            var strong = new CombatControlProfile(slowFraction: 0.5f, slowSeconds: 1f);
            Fire(Burst(new ProjectileBurstEffect(1, ProjectileLayout.Single, 0f, 0, 8f, 1f, 0.2f), strong),
                new SkillMechanicBonus(grantedSlowFraction: 0.3f, grantedSlowSeconds: 1.5f));
            Assert.AreEqual(0.5f, _launcher.Projectiles[1].Damage.Combat.Controls.SlowFraction, 1e-5f);
        }

        [Test]
        public void Cone_ContainsOnlyOffsetsWithinTheHalfAngle()
        {
            Assert.IsTrue(EnemyDamageArea.InsideCone(new Vector2(2f, 0.5f), Vector2.right, 30f));
            Assert.IsTrue(EnemyDamageArea.InsideCone(new Vector2(1f, 1.7320508f), Vector2.right, 60f), "Boundary is inside.");
            Assert.IsFalse(EnemyDamageArea.InsideCone(new Vector2(-1f, 0f), Vector2.right, 30f));
            Assert.IsFalse(EnemyDamageArea.InsideCone(new Vector2(0f, 2f), Vector2.right, 30f));
            Assert.IsTrue(EnemyDamageArea.InsideCone(Vector2.zero, Vector2.right, 30f), "Touching the caster counts.");
        }

        [Test]
        public void AreaEffect_ArcValidation()
        {
            Assert.AreEqual(60f, new AreaEffect(3.5f, 1f, 0f, 60f).ArcDegrees);
            Assert.AreEqual(0f, new AreaEffect(3.5f).ArcDegrees, "Neutral default is the full disk.");
            Assert.Throws<System.ArgumentException>(() => new AreaEffect(3.5f, 1f, 0.2f, 60f));
            Assert.Catch<System.ArgumentException>(() => new AreaEffect(3.5f, 1f, 0f, 400f));
        }

        [Test]
        public void ConeSetAttack_IsAnInstantNearestTargetCone()
        {
            var template = ProductionSetAttackCatalog.Create().Single(t => t.Id.ToString() == "SET-022-ATTACK");
            var level = template.GetLevel(1);
            Assert.AreEqual(ActiveSkillTargetingMode.NearestEnemy, level.TargetingMode);
            var area = (AreaEffect)level.Waves.Single().Effects.Single();
            Assert.AreEqual(60f, area.ArcDegrees);
            Assert.AreEqual(3.5f, area.Radius, 1e-5f);
            Assert.AreEqual(20f, level.BaseDamage);
            Assert.AreEqual(3f, level.Waves[0].Controls.KnockbackDistance, 1e-5f, "Strong knockback is the point of the set.");
            Assert.IsTrue(level.Targeting.RandomSeed.HasValue, "The no-target direction is seeded content.");
        }

        [Test]
        public void ConeDirection_FollowsTheTarget_OrPicksASeededRandomWayWithoutOne()
        {
            Assert.AreEqual(Vector2.up, EnemyDamageArea.ConeDirection(Vector2.up, true, null), "An aimed cone ignores the random.");
            var first = EnemyDamageArea.ConeDirection(Vector2.right, false, new System.Random(7));
            Assert.AreEqual(1f, first.magnitude, 1e-5f);
            Assert.AreEqual(first, EnemyDamageArea.ConeDirection(Vector2.right, false, new System.Random(7)), "Same seed, same direction.");
            var random = new System.Random(7);
            var directions = Enumerable.Range(0, 64).Select(_ => EnemyDamageArea.ConeDirection(Vector2.right, false, random)).ToArray();
            Assert.IsTrue(directions.Any(d => d.x < -0.5f) && directions.Any(d => d.y > 0.5f) && directions.Any(d => d.y < -0.5f),
                "Without an enemy the cone no longer repeats the previous aim.");
            Assert.Throws<System.ArgumentNullException>(() => EnemyDamageArea.ConeDirection(Vector2.right, false, null));
        }

        [Test]
        public void Cone_WithoutASkillSeed_IsRejected()
        {
            var cone = new ActiveSkillLevelDefinition(10f, 1f, ActiveSkillTargetingMode.NearestEnemy,
                new ActiveSkillActivationWave(0f, 0f, 1f, CombatControlProfile.None, new AreaEffect(3.5f, 1f, 0f, 60f)));
            Assert.Throws<System.ArgumentException>(() => new ActiveSkillProgressionDefinition(new ContentId("SET-TEST-CONE"), "Cone",
                cone, cone, cone, cone, cone, cone));
        }

        private static readonly ContentId SkillId = new ContentId("SKILL-013");

        private static ActiveSkillLevelDefinition Burst(IActiveSkillEffect effect, CombatControlProfile controls = null) =>
            new ActiveSkillLevelDefinition(10f, 1f, ActiveSkillTargetingMode.Self,
                new ActiveSkillActivationWave(0f, 0f, 1f, controls ?? CombatControlProfile.None, effect));

        private void Fire(ActiveSkillLevelDefinition level, SkillMechanicBonus mechanics)
        {
            _executor.Schedule(new ActiveSkillActivation(SkillId, 1, Vector2.zero, Vector2.right, null, 10f, level,
                _context.Owner.transform, hitLedger: new SkillHitLedger(), mechanics: mechanics));
            _executor.Tick(0f, true);
        }
    }
}

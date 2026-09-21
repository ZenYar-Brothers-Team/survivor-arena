using System.Linq;
using Game.Combat;
using Game.Enemy;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    public sealed class BossSkillTargetTests
    {
        [Test]
        public void EveryFixtureSkill_DiscoversBossAndDealsAttributedDamageThroughSharedPipeline()
        {
            var baseline = EnemyRegistry.Count;
            using var context = new SkillFrameworkTestContext();
            foreach (var skill in FixtureActiveSkillCatalog.Create())
            {
                var level = skill.GetLevel(1);
                var orbit = level.Waves.SelectMany(w => w.Effects).OfType<OrbitEffect>().FirstOrDefault();
                var position = Vector2.right * (orbit?.Radius ?? .25f);
                var boss = EnemyFactory.Spawn(new EnemyDefinition("FIXTURE-BOSS-SKILL-TARGET", 10000, 2.4f, 0, 0, 1),
                    position, context.Owner.transform, context.Run, category: EnemyCategory.Boss);
                try
                {
                    Physics2D.SyncTransforms();
                    Assert.IsTrue(new SceneEnemyTargetProvider().TryGetTarget(Vector2.zero, out var target), skill.Id.ToString());
                    Assert.AreSame(boss, target);
                    CombatResult hit = default;
                    boss.CombatResolved += result => hit = result;
                    var launcher = new CombatRecordingLauncher();
                    using var executor = new SceneActiveSkillEffectExecutor(context.Run, launcher);
                    executor.Schedule(new ActiveSkillActivation(skill.Id, 1, Vector2.zero, Vector2.right, boss,
                        level.BaseDamage, level, context.Owner.transform, random: new System.Random(42), hitLedger: new SkillHitLedger()));
                    for (var i = 0; i < 200; i++) executor.Tick(.02f, true);
                    // Projectile collision dispatch itself is tested here; movement/physics has independent PlayMode coverage.
                    foreach (var projectile in launcher.Projectiles)
                    {
                        var runtime = FixtureProjectileFactory.Spawn(projectile, context.Run);
                        try { runtime.TryImpact(boss, boss.Position); }
                        finally { runtime.Shutdown(); if (runtime != null) Object.DestroyImmediate(runtime.gameObject); }
                    }
                    Assert.Less(boss.Health.CurrentHealth, boss.Health.MaxHealth, skill.Id.ToString());
                    Assert.AreEqual(CombatEntityCategory.Boss, hit.Target.Category, skill.Id.ToString());
                    Assert.AreEqual(skill.Id, hit.Source.ContentId);
                }
                finally { if (boss != null) boss.Despawn(); }
            }
            Assert.AreEqual(baseline, EnemyRegistry.Count);
        }
    }
}

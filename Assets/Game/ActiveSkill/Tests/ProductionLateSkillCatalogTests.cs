using System.IO;
using System.Linq;
using Game.Content;
using Game.Presentation;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    /// <summary>
    /// IP-17 remainder: SKILL-008/009/011/012/015/016 mapped from the approved late-skills-passives-v1 data
    /// (DECISION-0060). Card numbers and level steps come from Content Design; added geometry from the packet.
    /// </summary>
    public sealed class ProductionLateSkillCatalogTests
    {
        private static ActiveSkillProgressionDefinition Skill(string id) =>
            ProductionActiveSkillCatalog.Create().Single(s => s.Id.ToString() == id);

        private static T Effect<T>(string id, int level, int wave = 0) where T : class, IActiveSkillEffect =>
            (T)Skill(id).GetLevel(level).Waves[wave].Effects[0];

        [Test]
        public void RicochetDisk_HitCountsFollowTheCard_AndTravelBudgetCoversEveryHop()
        {
            var l1 = Skill("SKILL-008").GetLevel(1);
            Assert.AreEqual(12.6f, l1.BaseDamage, 1e-4f, "DECISION-0073: 18 × 0.7.");
            Assert.AreEqual(2f, l1.CooldownSeconds);
            Assert.AreEqual(ActiveSkillTargetingMode.NearestEnemy, l1.TargetingMode);
            CollectionAssert.AreEqual(new[] { 1, 2, 3, 5, 5, 7 },
                Enumerable.Range(1, 6).Select(level => Effect<ProjectileBurstEffect>("SKILL-008", level).Behavior.RicochetCount));
            CollectionAssert.AreEqual(new[] { 9f, 12f, 15f }, Enumerable.Range(1, 3)
                .Select(level => (float)System.Math.Round(Effect<ProjectileBurstEffect>("SKILL-008", level).Speed
                    * Effect<ProjectileBurstEffect>("SKILL-008", level).LifetimeSeconds, 3)));
            var l6 = Effect<ProjectileBurstEffect>("SKILL-008", 6);
            Assert.IsTrue(l6.Behavior.RepeatRicochetTargets, "Repeats a target only when no other valid target exists.");
            Assert.AreEqual(27f, l6.Speed * l6.LifetimeSeconds, 1e-3f, "Travel budget 6 + 3 x 7 ricochets.");
            Assert.AreEqual(0.162f, Effect<ProjectileBurstEffect>("SKILL-008", 1).CollisionRadius, 1e-5f);
            Assert.AreEqual(0.1944f, l6.CollisionRadius, 1e-5f);
            Assert.AreEqual(16.38f, Skill("SKILL-008").GetLevel(3).BaseDamage, 1e-4f);
            Assert.AreEqual(0.3625f, Skill("SKILL-008").GetLevel(5).Waves[0].Controls.KnockbackDistance, 1e-5f);
        }

        [Test]
        public void RicochetDisk_VisibleCircleMatchesHitRadius()
        {
            var visual = FixtureSpriteCatalog.CreateFor(new ContentId[] { "SKILL-008-VISUAL-PROJECTILE" })[0];
            var radius = Effect<ProjectileBurstEffect>("SKILL-008", 1).CollisionRadius;
            var texture = new Texture2D(2, 2);
            try
            {
                Assert.IsTrue(texture.LoadImage(File.ReadAllBytes(
                    "Assets/Resources/Art/Sprites/Skills/skill-008/skill-008-projectile.png")));
                var pixels = texture.GetPixels32();
                var minX = texture.width;
                var minY = texture.height;
                var maxX = -1;
                var maxY = -1;
                for (var y = 0; y < texture.height; y++)
                    for (var x = 0; x < texture.width; x++)
                    {
                        if (pixels[y * texture.width + x].a < 16) continue;
                        minX = Mathf.Min(minX, x);
                        minY = Mathf.Min(minY, y);
                        maxX = Mathf.Max(maxX, x);
                        maxY = Mathf.Max(maxY, y);
                    }
                Assert.GreaterOrEqual(maxX, minX, "The disk must have an opaque silhouette.");
                Assert.AreEqual(texture.width, minX + maxX + 1, "The disk must rotate around its visual center.");
                Assert.AreEqual(texture.height, minY + maxY + 1);
                var visibleRadiusX = radius * visual.ProjectilePresentation.VisualScale *
                                     (maxX - minX + 1f) / texture.width;
                var visibleRadiusY = radius * visual.ProjectilePresentation.VisualScale *
                                     (maxY - minY + 1f) / texture.height;
                var tolerance = 1f / visual.Sprite.pixelsPerUnit;
                Assert.That(visibleRadiusX, Is.EqualTo(radius).Within(tolerance));
                Assert.That(visibleRadiusY, Is.EqualTo(radius).Within(tolerance));
            }
            finally { Object.DestroyImmediate(texture); }
        }

        [Test]
        public void MagmaMine_CapGrowsAtL4_AndL6AddsTheSecondaryBlast()
        {
            var l1 = Effect<MineEffect>("SKILL-009", 1);
            Assert.AreEqual(24f, Skill("SKILL-009").GetLevel(1).BaseDamage, 1e-4f);
            CollectionAssert.AreEqual(new[] { 0.75f, 1.5f, 1.875f }, Enumerable.Range(1, 3)
                .Select(level => Effect<MineEffect>("SKILL-009", level).BlastRadius));
            Assert.AreEqual(4, l1.MaxConcurrent);
            Assert.AreEqual(6f, l1.LifetimeSeconds, 1e-5f);
            Assert.AreEqual(0f, l1.SecondaryDamageMultiplier);
            var l4 = Effect<MineEffect>("SKILL-009", 4);
            Assert.AreEqual(6, l4.MaxConcurrent);
            Assert.AreEqual(7.2f, l4.LifetimeSeconds, 1e-5f);
            var l6 = Effect<MineEffect>("SKILL-009", 6);
            Assert.AreEqual(2.1f, l6.BlastRadius, 1e-5f);
            Assert.AreEqual(0.4f, l6.SecondaryDelaySeconds, 1e-5f);
            Assert.AreEqual(0.75f, l6.SecondaryRadiusMultiplier, 1e-5f);
            Assert.AreEqual(0.6f, l6.SecondaryDamageMultiplier, 1e-5f);
            Assert.AreEqual(0.7f, l6.SecondaryKnockbackMultiplier, 1e-5f);
            Assert.AreEqual(0.98f, Skill("SKILL-009").GetLevel(6).Waves[0].Controls.KnockbackDistance, 1e-5f);
        }

        [Test]
        public void ShardSpiral_RotatesEachActivation_AndL6FiresASecondHalfStepRing()
        {
            var l1 = Skill("SKILL-011").GetLevel(1);
            Assert.AreEqual(15f, l1.Targeting.RotationPerActivationDegrees, 1e-5f);
            CollectionAssert.AreEqual(new[] { 4, 7, 10, 12, 12, 12 },
                Enumerable.Range(1, 6).Select(level => Effect<ProjectileBurstEffect>("SKILL-011", level).ProjectileCount));
            Assert.AreEqual(ProjectileLayout.Ring, Effect<ProjectileBurstEffect>("SKILL-011", 1).Layout);
            var l6 = Skill("SKILL-011").GetLevel(6);
            Assert.AreEqual(2, l6.Waves.Count);
            Assert.AreEqual(15f, l6.Waves[1].RotationDegrees, 1e-5f, "Half of the 30° step between 12 shards.");
            Assert.AreEqual(0.15f, l6.Waves[1].DelaySeconds, 1e-5f);
            var l4 = Effect<ProjectileBurstEffect>("SKILL-011", 4);
            Assert.AreEqual(5.175f, l4.Speed * l4.LifetimeSeconds, 1e-3f, "Faster shards keep the same reach.");
        }

        [Test]
        public void PulseBeam_TicksEveryFifthOfASecond_AndEveryLevelTracksTheTarget()
        {
            var l1 = Effect<BeamEffect>("SKILL-012", 1);
            Assert.AreEqual(7.5f, Skill("SKILL-012").GetLevel(1).BaseDamage, 1e-4f);
            Assert.AreEqual(0.2f, l1.TickIntervalSeconds, 1e-5f);
            CollectionAssert.AreEqual(new[] { 0.4f, 0.8f, 1.1f }, Enumerable.Range(1, 3)
                .Select(level => Effect<BeamEffect>("SKILL-012", level).DurationSeconds));
            Assert.AreEqual(6f, l1.Range, 1e-5f);
            Assert.AreEqual(0.26f, l1.Width, 1e-5f);
            Assert.AreEqual(0.04f, Skill("SKILL-012").GetLevel(1).Waves[0].Controls.KnockbackDistance, 1e-5f);
            // DECISION-0144: the beam follows its living target from L1.
            Assert.IsTrue(Enumerable.Range(1, 6).All(level => Effect<BeamEffect>("SKILL-012", level).TracksTarget));
            var l6 = Effect<BeamEffect>("SKILL-012", 6);
            Assert.AreEqual(1.5f, l6.DurationSeconds, 1e-5f);
            Assert.AreEqual(0.442f, l6.Width, 1e-5f);
            Assert.AreEqual(7.2f, l6.Range, 1e-5f);
        }

        [Test]
        public void BladeCross_FixedAxes_EightAtL4_AndL6RepeatsRotatedCross()
        {
            var l1 = Effect<ProjectileBurstEffect>("SKILL-015", 1);
            Assert.AreEqual(ProjectileLayout.Cross, l1.Layout);
            Assert.AreEqual(4, l1.ProjectileCount);
            CollectionAssert.AreEqual(new[] { 2f, 4f, 5.2f }, Enumerable.Range(1, 3)
                .Select(level => (float)System.Math.Round(Effect<ProjectileBurstEffect>("SKILL-015", level).Speed
                    * Effect<ProjectileBurstEffect>("SKILL-015", level).LifetimeSeconds, 3)));
            Assert.IsTrue(l1.Behavior.UnlimitedPierce, "Each wave cuts through every enemy once.");
            Assert.AreEqual(ActiveSkillTargetingMode.Self, Skill("SKILL-015").GetLevel(1).TargetingMode);
            Assert.AreEqual(0f, Skill("SKILL-015").GetLevel(1).Targeting.RotationPerActivationDegrees);
            Assert.AreEqual(8, Effect<ProjectileBurstEffect>("SKILL-015", 4).ProjectileCount);
            var l6 = Skill("SKILL-015").GetLevel(6);
            Assert.AreEqual(2, l6.Waves.Count);
            Assert.AreEqual(22.5f, l6.Waves[1].RotationDegrees, 1e-5f);
            Assert.AreEqual(0.35f, l6.Waves[1].DelaySeconds, 1e-5f);
            Assert.AreEqual(1f, l6.Waves[1].DamageMultiplier, 1e-5f);
            Assert.AreEqual(l6.Waves[0].Controls.KnockbackDistance, l6.Waves[1].Controls.KnockbackDistance, 1e-5f);
        }

        [Test]
        public void JunkScatter_DeceleratesToAStop_AndGrowsCountAndPierce()
        {
            var l1 = Effect<ProjectileBurstEffect>("SKILL-016", 1);
            Assert.AreEqual(0.35f, Skill("SKILL-016").GetLevel(1).CooldownSeconds, 1e-5f);
            Assert.AreEqual(ProjectileLayout.IndependentRandom, l1.Layout);
            Assert.IsTrue(Skill("SKILL-016").GetLevel(1).Targeting.RandomSeed.HasValue, "Random directions need a JSON seed.");
            Assert.AreEqual(8f, l1.Speed, 1e-5f);
            CollectionAssert.AreEqual(new[] { 0.7f, 1.4f, 1.68f }, Enumerable.Range(1, 3)
                .Select(level => Effect<ProjectileBurstEffect>("SKILL-016", level).Behavior.StopAfterSeconds));
            CollectionAssert.AreEqual(new[] { 1, 1, 2, 2, 2, 3 },
                Enumerable.Range(1, 6).Select(level => Effect<ProjectileBurstEffect>("SKILL-016", level).ProjectileCount));
            Assert.AreEqual(1, Effect<ProjectileBurstEffect>("SKILL-016", 4).PierceCount);
            Assert.AreEqual(3.92f, Skill("SKILL-016").GetLevel(6).BaseDamage, 1e-4f);
        }
    }
}

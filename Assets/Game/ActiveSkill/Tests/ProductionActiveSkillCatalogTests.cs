using System.Linq;
using Game.Content;
using Game.Presentation;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using UnityEngine;

namespace Game.ActiveSkill.Tests
{
    /// <summary>F1-01: production SKILL-001…007/010/013/014 mapped from the approved FIELD-001 baseline v1.</summary>
    public sealed class ProductionActiveSkillCatalogTests
    {
        private static ActiveSkillProgressionDefinition Skill(string id) =>
            ProductionActiveSkillCatalog.Create().Single(s => s.Id.ToString() == id);

        private static T Effect<T>(string id, int level, int wave = 0) where T : class, IActiveSkillEffect =>
            (T)Skill(id).GetLevel(level).Waves[wave].Effects[0];

        [Test]
        public void Catalog_ContainsExactlyTheTenStartupSkills_WithSixLevelsAndIcons()
        {
            var skills = ProductionActiveSkillCatalog.Create();
            CollectionAssert.AreEqual(new[] { "SKILL-001", "SKILL-002", "SKILL-003", "SKILL-004", "SKILL-005",
                "SKILL-006", "SKILL-007", "SKILL-010", "SKILL-013", "SKILL-014" }, skills.Select(s => s.Id.ToString()));
            foreach (var skill in skills)
            {
                Assert.AreEqual(6, skill.Levels.Count, skill.Id.ToString());
                Assert.AreEqual(skill.Id + "-VISUAL-ICON", skill.Icon.Id.ToString());
            }
        }

        [Test]
        public void Stone_LevelsKeepCardSemantics()
        {
            var l1 = Skill("SKILL-001").GetLevel(1);
            Assert.AreEqual(20f, l1.BaseDamage);
            Assert.AreEqual(1.2f, l1.CooldownSeconds, 1e-5f);
            Assert.AreEqual(5f, l1.Targeting.Radius);
            Assert.AreEqual(0.35f, l1.Waves[0].Controls.KnockbackDistance, 1e-5f);
            Assert.AreEqual(0.12f, l1.Waves[0].Controls.KnockbackSeconds, 1e-5f);
            var l4 = Effect<ProjectileBurstEffect>("SKILL-001", 4);
            Assert.AreEqual(2, l4.ProjectileCount);
            Assert.AreEqual(1, l4.Behavior.RicochetCount);
            Assert.AreEqual(0.8f, l4.Behavior.RicochetRetention, 1e-5f);
            Assert.IsFalse(l4.Behavior.RepeatRicochetTargets);
            Assert.IsTrue(l4.Behavior.DistinctNearestTargets);
            var l6 = Skill("SKILL-001").GetLevel(6);
            Assert.AreEqual(31f, l6.BaseDamage, 1e-4f);
            Assert.AreEqual(0.33f, l6.Targeting.ActionSpeedBonus, 1e-5f);
            Assert.AreEqual(3, ((ProjectileBurstEffect)l6.Waves[0].Effects[0]).ProjectileCount);
            Assert.AreEqual(0.192f, ((ProjectileBurstEffect)l6.Waves[0].Effects[0]).CollisionRadius, 1e-5f);
            Assert.AreEqual(5f / 11.5f, ((ProjectileBurstEffect)l6.Waves[0].Effects[0]).LifetimeSeconds, 1e-4f);
        }

        [Test]
        public void NeedlesSpearAndShards_MapMaxTargetsToPierceAndSlow()
        {
            Assert.AreEqual(1, Effect<ProjectileBurstEffect>("SKILL-002", 6).PierceCount);
            Assert.AreEqual(11, Effect<ProjectileBurstEffect>("SKILL-002", 6).ProjectileCount);
            Assert.AreEqual(90f, Effect<ProjectileBurstEffect>("SKILL-002", 6).SpreadDegrees);
            Assert.AreEqual(2, Effect<ProjectileBurstEffect>("SKILL-005", 1).PierceCount);
            Assert.AreEqual(4, Effect<ProjectileBurstEffect>("SKILL-005", 3).PierceCount);
            Assert.IsTrue(Effect<ProjectileBurstEffect>("SKILL-005", 6).Behavior.UnlimitedPierce);
            Assert.AreEqual(ActiveSkillTargetingMode.MovementDirection, Skill("SKILL-005").GetLevel(1).TargetingMode);
            var shards = Skill("SKILL-013").GetLevel(6);
            Assert.AreEqual(13, ((ProjectileBurstEffect)shards.Waves[0].Effects[0]).ProjectileCount);
            Assert.AreEqual(100f, ((ProjectileBurstEffect)shards.Waves[0].Effects[0]).SpreadDegrees);
            Assert.AreEqual(0.3f, shards.Waves[0].Controls.SlowFraction, 1e-5f);
            Assert.AreEqual(2f, shards.Waves[0].Controls.SlowSeconds, 1e-5f);
        }

        [Test]
        public void OrbitBlades_ArePersistentWithPerBladeHitCooldown()
        {
            for (var level = 1; level <= 6; level++)
                Assert.IsTrue(Effect<OrbitEffect>("SKILL-003", level).Persistent);
            var l6 = Effect<OrbitEffect>("SKILL-003", 6);
            Assert.AreEqual(4, l6.BladeCount);
            Assert.AreEqual(2.25f, l6.Radius, 1e-5f);
            Assert.AreEqual(0.496f, l6.BladeHitboxRadius, 1e-5f);
            Assert.AreEqual(180f, l6.AngularSpeedDegrees);
            Assert.AreEqual(0.6f, l6.HitCooldownSeconds, 1e-5f);
            var preview = Skill("SKILL-003").CreateDraftPreview(2, 3);
            Assert.IsFalse(preview.Values.Any(v => v.Label == "Base cooldown"), "Refresh interval is not a player-facing cooldown.");
        }

        [Test]
        public void PulseWave_ExpandsAndAddsHalfDamageSecondWaveFromLevelFour()
        {
            var l3 = Skill("SKILL-004").GetLevel(3);
            Assert.AreEqual(1, l3.Waves.Count);
            Assert.AreEqual(0.25f, ((AreaEffect)l3.Waves[0].Effects[0]).ExpansionSeconds, 1e-5f);
            var l4 = Skill("SKILL-004").GetLevel(4);
            Assert.AreEqual(2, l4.Waves.Count);
            Assert.AreEqual(0.35f, l4.Waves[1].DelaySeconds, 1e-5f);
            Assert.AreEqual(0.5f, l4.Waves[1].DamageMultiplier, 1e-5f);
            Assert.AreEqual(3.125f * 0.7f, ((AreaEffect)l4.Waves[1].Effects[0]).Radius, 1e-4f);
            Assert.AreEqual(2.4f * 0.6f, l4.Waves[1].Controls.KnockbackDistance, 1e-4f);
            var l6 = Skill("SKILL-004").GetLevel(6);
            Assert.AreEqual(3.5f, ((AreaEffect)l6.Waves[1].Effects[0]).Radius, 1e-4f);
            Assert.AreEqual(2.72f, l6.Waves[1].Controls.KnockbackDistance, 1e-4f);
        }

        [Test]
        public void SkyStrike_UsesSequentialTelegraphedStrikesWithLargerThird()
        {
            var l1 = Skill("SKILL-010").GetLevel(1);
            Assert.AreEqual(ActiveSkillTargetingMode.RandomEnemy, l1.TargetingMode);
            Assert.AreEqual(8f, l1.Targeting.Radius);
            Assert.IsTrue(l1.Targeting.RandomSeed.HasValue);
            Assert.AreEqual(0.6f, ((StrikeEffect)l1.Waves[0].Effects[0]).TelegraphSeconds, 1e-5f);
            var l6 = Skill("SKILL-010").GetLevel(6);
            CollectionAssert.AreEqual(new[] { 0f, 0.3f, 0.6f }, l6.Waves.Select(w => (float)System.Math.Round(w.DelaySeconds, 4)));
            Assert.AreEqual(0.45f, ((StrikeEffect)l6.Waves[0].Effects[0]).TelegraphSeconds, 1e-5f);
            Assert.AreEqual(2.24f * 1.5f, ((StrikeEffect)l6.Waves[2].Effects[0]).Radius, 1e-4f);
            Assert.AreEqual(0.54f * 1.5f, l6.Waves[2].Controls.KnockbackDistance, 1e-4f);
            Assert.IsTrue(Enumerable.Range(1, 6).SelectMany(level => Skill("SKILL-010").GetLevel(level).Waves)
                .All(wave => ((StrikeEffect)wave.Effects[0]).VerticalScale == 0.7f), "DECISION-0058: flattened ground area.");
        }

        [Test]
        public void BoomerangChainAndSpheres_KeepReturnChainAndExplosionRatios()
        {
            Assert.AreEqual(1.75f, Effect<BoomerangEffect>("SKILL-006", 6).ReturnDamageMultiplier, 1e-5f);
            Assert.AreEqual(1f, Effect<BoomerangEffect>("SKILL-006", 1).HitCooldownSeconds, 1e-5f);
            Assert.AreEqual(8, Effect<ChainEffect>("SKILL-007", 6).TargetCount);
            Assert.AreEqual(0.95f, Effect<ChainEffect>("SKILL-007", 6).DamageRetentionPerJump, 1e-5f);
            Assert.AreEqual(0f, Skill("SKILL-007").GetLevel(1).Waves[0].Controls.KnockbackDistance);
            var spheres = Effect<ProjectileBurstEffect>("SKILL-014", 6);
            Assert.AreEqual(ProjectileLayout.IndependentRandom, spheres.Layout);
            Assert.AreEqual(41.6f / 21.6f, spheres.Behavior.ExplosionDamageMultiplier, 1e-4f);
            Assert.AreEqual(1.05f / 0.25f, spheres.Behavior.ExplosionKnockbackMultiplier, 1e-4f);
            Assert.IsTrue(spheres.Behavior.ExplodeOnExpiry);
            Assert.AreEqual(1, spheres.PierceCount, "L6 passes the first target and explodes on the second.");
            Assert.AreEqual(1.885f, spheres.ImpactAreaRadius, 1e-4f);
        }

        [Test]
        public void VisualReferences_AreRegisteredProjectileSprites_OrProceduralWorldEffects()
        {
            var sprites = JArray.Parse(Resources.Load<TextAsset>("Content/Presentation/FixtureSprites").text)
                .ToDictionary(t => (string)t["id"], t => (string)t["role"]);
            var worldEffects = SkillWorldEffectCatalog.Create();
            foreach (var skill in ProductionActiveSkillCatalog.Create())
            {
                Assert.AreEqual("Icon", sprites[skill.Icon.Id.ToString()], skill.Id.ToString());
                var visual = skill.GetLevel(1).Visual.Id;
                if (visual.IsValid) Assert.AreEqual("Projectile", sprites[visual.ToString()], skill.Id.ToString());
                else Assert.IsTrue(worldEffects.ContainsKey(skill.Id), $"{skill.Id} needs a sprite or a procedural world effect.");
            }
            Assert.AreEqual(SkillWorldEffectKind.ExpandingRing, worldEffects[new ContentId("SKILL-004")].Kind);
            Assert.AreEqual(SkillWorldEffectKind.ChainArc, worldEffects[new ContentId("SKILL-007")].Kind);
            Assert.AreEqual(SkillWorldEffectKind.StrikeTelegraph, worldEffects[new ContentId("SKILL-010")].Kind);
        }

        [Test]
        public void ProductionVisuals_ResolveWithoutFixtureFallback()
        {
            var ids = ProductionActiveSkillCatalog.Create()
                .SelectMany(s => new[] { s.Icon.Id, s.GetLevel(1).Visual.Id }).Where(id => id.IsValid).ToArray();
            var sprites = FixtureSpriteCatalog.CreateFor(ids);
            Assert.AreEqual(ids.Distinct().Count(), sprites.Count);
            Assert.IsTrue(sprites.All(s => s.Sprite != null));
        }

        [Test]
        public void FixtureIds_AreRejectedByProductionCatalog()
        {
            Assert.Throws<System.InvalidOperationException>(() => ProductionActiveSkillCatalog.FromJson(
                "[{\"id\":\"FIXTURE-SKILL-X\",\"displayName\":\"x\",\"iconVisualId\":\"X-ICON\",\"levels\":[" +
                string.Join(",", Enumerable.Repeat("{\"baseDamage\":1,\"cooldownSeconds\":1,\"targetingMode\":\"Self\",\"waves\":[{\"effects\":[{\"kind\":\"Area\",\"radius\":1}]}]}", 6)) + "]}]"));
        }
    }
}

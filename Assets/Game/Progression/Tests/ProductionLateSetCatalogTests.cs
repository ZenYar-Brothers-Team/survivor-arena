using System;
using System.Linq;
using Game.Content;
using NUnit.Framework;

namespace Game.Progression.Tests
{
    /// <summary>IP-19 remainder: the 15 sets from sets-v1 (DECISION-0061) — thresholds, effect mapping and ownership.</summary>
    public sealed class ProductionLateSetCatalogTests
    {
        private static SetDefinition Set(string id) => ProductionSetCatalog.Create().Single(s => s.Id.ToString() == id);

        private static int Threshold(SetDefinition set, string component) =>
            set.Recipe.Single(c => c.Id.ToString() == component).MinimumLevel;

        private static SkillMechanicBonus Mechanics(SetDefinition set, string skill) =>
            set.Effects.Where(e => e.Kind == SetEffectKind.SkillMechanics && e.Skill.Value.ToString() == skill)
                .Aggregate(default(SkillMechanicBonus), (total, e) => total.Plus(e.Mechanics));

        [Test]
        public void Thresholds_FollowTheApprovedPacket()
        {
            Assert.AreEqual(3, Threshold(Set("SET-002"), "SKILL-008"));
            Assert.AreEqual(4, Threshold(Set("SET-003"), "SKILL-007"));
            Assert.AreEqual(3, Threshold(Set("SET-005"), "PASSIVE-010"));
            Assert.AreEqual(3, Threshold(Set("SET-011"), "SKILL-001"));
            Assert.AreEqual(4, Threshold(Set("SET-013"), "SKILL-012"));
            Assert.AreEqual(2, Threshold(Set("SET-014"), "PASSIVE-005"));
            Assert.AreEqual(3, Threshold(Set("SET-015"), "SKILL-016"));
            Assert.AreEqual(4, Threshold(Set("SET-020"), "SKILL-001"));
            foreach (var set in ProductionSetCatalog.Create())
            {
                Assert.AreEqual(set.Id + "-VISUAL-ICON", set.Icon.Id.ToString());
                // Component buffs never reach skills outside the recipe (CD «Sets» general rules).
                foreach (var effect in set.Effects.Where(e => e.Skill.HasValue))
                    Assert.IsTrue(set.Recipe.Any(c => c.Id == effect.Skill.Value), $"{set.Id} buffs {effect.Skill}");
            }
        }

        [Test]
        public void MechanicSets_CarryTheChosenDecisions()
        {
            Assert.AreEqual(0.5f, Mechanics(Set("SET-002"), "SKILL-008").ReturnDamageBonus, 1e-5f, "G-04: disc rebounds.");
            Assert.AreEqual(0.3f, Mechanics(Set("SET-002"), "SKILL-006").ReturnSpeedBonus, 1e-5f);
            var chain = Mechanics(Set("SET-003"), "SKILL-007");
            Assert.AreEqual(2, chain.ExtraChainTargets);
            Assert.AreEqual(0.3f, chain.ChainJumpRangeBonus, 1e-5f);
            Assert.AreEqual(0.5f, chain.ChainFalloffReduction, 1e-5f);
            var junk = Mechanics(Set("SET-008"), "SKILL-016");
            Assert.AreEqual(8, junk.HeavyEveryNth);
            Assert.AreEqual(0.15f, junk.ExplosionRadiusBonus, 1e-5f);
            Assert.AreEqual(2, Mechanics(Set("SET-009"), "SKILL-005").ExtraPierce);
            Assert.AreEqual(0.3f, Mechanics(Set("SET-011"), "SKILL-008").ProjectileSpeedBonus, 1e-5f);
            Assert.AreEqual(0.35f, Mechanics(Set("SET-014"), "SKILL-003").OrbitAngularSpeedBonus, 1e-5f, "User: +35% rotation.");
            Assert.AreEqual(0.35f, Mechanics(Set("SET-015"), "SKILL-009").ExplosionDamageBonus, 1e-5f);
            var junkDamage = Set("SET-015").Effects.Single(e => e.Kind == SetEffectKind.SkillTransform && e.Skill.Value.ToString() == "SKILL-016");
            Assert.AreEqual(0.5f, junkDamage.Modifier.ActiveSkillDamageMultiplierBonus, 1e-5f, "G-05: +50% junk damage.");
            Assert.AreEqual(0f, Mechanics(Set("SET-015"), "SKILL-016").ExplosionDamageBonus, "No heavy-junk amplification.");
        }

        [Test]
        public void ProgressionAndDefenceSets_UseStatBuffsAndLevelHeal()
        {
            var greed = Set("SET-005");
            var stats = greed.Effects.Single(e => e.Kind == SetEffectKind.StatBuff).Modifier;
            Assert.AreEqual(0.2f, stats.DisappearingXpRecoveryBonus, 1e-5f);
            Assert.AreEqual(0.3f, stats.PickupRadiusMultiplierBonus, 1e-5f);
            Assert.AreEqual(0.15f, stats.PickedUpXpMultiplierBonus, 1e-5f);
            Assert.AreEqual(0.05f, greed.Effects.Single(e => e.Kind == SetEffectKind.LevelHeal).HealFraction, 1e-5f);
            var fortress = Set("SET-012").Effects.Single(e => e.Kind == SetEffectKind.StatBuff).Modifier;
            Assert.AreEqual(0.25f, fortress.MaxHealthMultiplierBonus, 1e-5f);
            Assert.AreEqual(0.1f, fortress.IncomingDamageReductionBonus, 1e-5f);
            Assert.AreEqual(0.3f, fortress.KnockbackResistanceBonus, 1e-5f);
        }

        [Test]
        public void SetAttacks_UseTheirTemplatesAndFixedCooldowns()
        {
            foreach (var (id, cooldown) in new[] { ("SET-013", 6f), ("SET-016", 5f), ("SET-018", 8f), ("SET-019", 6f), ("SET-020", 5f) })
            {
                var attack = Set(id).Effects.Single(e => e.Kind == SetEffectKind.IndependentAttack);
                Assert.AreEqual(id + "-ATTACK", attack.AttackTemplate.Value.ToString());
                Assert.AreEqual(cooldown, attack.CooldownSeconds, 1e-5f, id);
            }
            var potion = Set("SET-015").Effects.Single(e => e.Kind == SetEffectKind.RewardProc);
            Assert.AreEqual("SET-015-ATTACK", potion.AttackTemplate.Value.ToString());
            Assert.AreEqual(10f, potion.CooldownSeconds, 1e-5f);
            Assert.IsTrue(potion.ScalesWithSizeAndRange);
        }

        [Test]
        public void Ability_RegistersMechanics_AndRemovesThemOnDispose()
        {
            var host = new FakeSetEffectHost();
            var ability = new SetEffectAbility(Set("SET-008"), host);
            Assert.AreEqual(4, host.SkillMechanics.Count);
            Assert.AreEqual(1, host.SkillMechanics.Values.Count(m => m.bonus.HasHeavyReplacement));
            ability.Dispose();
            Assert.AreEqual(0, host.SkillMechanics.Count);
        }

        [Test]
        public void MechanicBonus_AddsUp_ComposesFalloff_AndRejectsTwoHeavyOwners()
        {
            var total = new SkillMechanicBonus(explosionRadiusBonus: 0.15f, chainFalloffReduction: 0.5f)
                .Plus(new SkillMechanicBonus(explosionRadiusBonus: 0.25f, chainFalloffReduction: 0.5f));
            Assert.AreEqual(0.4f, total.ExplosionRadiusBonus, 1e-5f);
            Assert.AreEqual(0.75f, total.ChainFalloffReduction, 1e-5f);
            var heavy = new SkillMechanicBonus(heavyEveryNth: 8, heavySizeMultiplier: 2f, heavyStopMultiplier: 1.5f,
                heavyExplosionRadius: 1.2f, heavyExplosionDamageMultiplier: 4f);
            Assert.Throws<InvalidOperationException>(() => heavy.Plus(heavy));
            Assert.Throws<ArgumentException>(() => new SkillMechanicBonus(heavySizeMultiplier: 2f));
            Assert.Throws<ArgumentException>(() => new SetEffectDefinition(SetEffectKind.SkillMechanics, skill: new ContentId("SKILL-016")),
                "An empty mechanics effect is a data error.");
        }
    }
}

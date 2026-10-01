using System.Linq;
using NUnit.Framework;

namespace Game.Progression.Tests
{
    /// <summary>Low-tier sets SET-021…035 (sets-low-v1, DECISION-0138): recipe shape, effect mapping and component scope.</summary>
    public sealed class ProductionLowSetCatalogTests
    {
        private static readonly string[] LowIds = System.Linq.Enumerable.Range(21, 15).Select(n => $"SET-{n:000}").ToArray();

        private static SetDefinition Set(string id) => ProductionSetCatalog.Create().Single(s => s.Id.ToString() == id);

        private static SkillMechanicBonus Mechanics(SetDefinition set, string skill) =>
            set.Effects.Where(e => e.Kind == SetEffectKind.SkillMechanics && e.Skill.Value.ToString() == skill)
                .Aggregate(default(SkillMechanicBonus), (total, e) => total.Plus(e.Mechanics));

        [Test]
        public void Recipes_AreTwoToFourComponents_WithLevelSumFiveToSix_AndDistinctFromEveryOtherSet()
        {
            var all = ProductionSetCatalog.Create();
            foreach (var id in LowIds)
            {
                var set = Set(id);
                Assert.That(set.Recipe.Count, Is.InRange(2, 4), id);
                Assert.That(set.Recipe.Sum(c => c.MinimumLevel), Is.InRange(5, 6), id);
                var components = set.Recipe.Select(c => c.Id.ToString()).OrderBy(c => c).ToArray();
                Assert.AreEqual(1, all.Count(o => o.Recipe.Select(c => c.Id.ToString()).OrderBy(c => c).SequenceEqual(components)), id + " recipe is unique");
                foreach (var effect in set.Effects.Where(e => e.Skill.HasValue))
                    Assert.IsTrue(set.Recipe.Any(c => c.Id == effect.Skill.Value), $"{id} buffs {effect.Skill}");
            }
        }

        [Test]
        public void PassiveSets_AreBuiltOnlyFromPassives()
        {
            foreach (var id in new[] { "SET-023", "SET-024", "SET-025" })
                Assert.IsTrue(Set(id).Recipe.All(c => c.Id.ToString().StartsWith("PASSIVE-")), id);
        }

        [Test]
        public void SkillSets_CarryTheApprovedBonuses()
        {
            Assert.AreEqual(0.25f, Mechanics(Set("SET-033"), "SKILL-003").OrbitAngularSpeedBonus, 1e-5f);
            Assert.AreEqual(2, Mechanics(Set("SET-034"), "SKILL-013").ExtraProjectiles);
            Assert.AreEqual(1, Mechanics(Set("SET-035"), "SKILL-002").ExtraPierce);
            Assert.AreEqual(0.05f, Mechanics(Set("SET-028"), "SKILL-013").SlowStrengthBonus, 1e-5f);
            Assert.AreEqual(0.15f, Mechanics(Set("SET-028"), "SKILL-007").ChainJumpRangeBonus, 1e-5f);
            Assert.AreEqual(0.15f, Mechanics(Set("SET-029"), "SKILL-001").ProjectileSpeedBonus, 1e-5f);
            Assert.AreEqual(0.15f, Mechanics(Set("SET-032"), "SKILL-014").ExplosionRadiusBonus, 1e-5f);
            var damage = Set("SET-026").Effects.Where(e => e.Kind == SetEffectKind.SkillTransform).ToArray();
            CollectionAssert.AreEquivalent(new[] { "SKILL-003", "SKILL-004" }, damage.Select(e => e.Skill.Value.ToString()));
            Assert.IsTrue(damage.All(e => System.Math.Abs(e.Modifier.ActiveSkillDamageMultiplierBonus - 0.15f) < 1e-5f));
        }

        [Test]
        public void GeneralSets_AreStatBuffs()
        {
            var buff = Set("SET-024").Effects.Single();
            Assert.AreEqual(SetEffectKind.StatBuff, buff.Kind);
            Assert.AreEqual(0.1f, buff.Modifier.ActiveSkillDamageMultiplierBonus, 1e-5f);
            Assert.AreEqual(0.06f, buff.Modifier.ActionSpeedBonus, 1e-5f);
        }

        [Test]
        public void SkillMechanicBonus_AddsExtraProjectilesAndCapsSlow()
        {
            var total = new SkillMechanicBonus(extraProjectiles: 2, slowStrengthBonus: 0.6f)
                .Plus(new SkillMechanicBonus(extraProjectiles: 1, slowStrengthBonus: 0.6f));
            Assert.AreEqual(3, total.ExtraProjectiles);
            Assert.AreEqual(1f, total.SlowStrengthBonus, 1e-5f);
        }
    }
}

using System.Linq;
using Game.Combat;
using Game.Content;
using NUnit.Framework;

namespace Game.Progression.Tests
{
    /// <summary>F1-05: production SET-001/004/006/010/017 recipes and effect wiring (baseline v1).</summary>
    public sealed class ProductionSetCatalogTests
    {
        private static SetDefinition Set(string id) => ProductionSetCatalog.Create().Single(s => s.Id.ToString() == id);

        private static int Threshold(SetDefinition set, string component) =>
            set.Recipe.Single(c => c.Id.ToString() == component).MinimumLevel;

        [Test]
        public void Catalog_ContainsTheFiveStartupSets_WithBaselineThresholds()
        {
            CollectionAssert.AreEqual(new[] { "SET-001", "SET-004", "SET-006", "SET-010", "SET-017" },
                ProductionSetCatalog.Create().Select(s => s.Id.ToString()));
            Assert.AreEqual(3, Threshold(Set("SET-001"), "SKILL-001"));
            Assert.AreEqual(2, Threshold(Set("SET-001"), "PASSIVE-004"));
            Assert.AreEqual(3, Threshold(Set("SET-004"), "SKILL-013"));
            Assert.AreEqual(3, Threshold(Set("SET-006"), "PASSIVE-002"));
            Assert.AreEqual(4, Threshold(Set("SET-010"), "SKILL-003"));
            Assert.AreEqual(2, Threshold(Set("SET-010"), "PASSIVE-008"));
            Assert.AreEqual(4, Threshold(Set("SET-017"), "SKILL-010"));
            Assert.AreEqual(3, Threshold(Set("SET-017"), "PASSIVE-005"));
            foreach (var set in ProductionSetCatalog.Create())
            {
                Assert.AreEqual(set.Id + "-VISUAL-ICON", set.Icon.Id.ToString());
                Assert.IsFalse(string.IsNullOrWhiteSpace(set.Description));
            }
        }

        [Test]
        public void CompatibleBuild_Set001_004_010_017_FitsSixPlusSix()
        {
            var components = new[] { "SET-001", "SET-004", "SET-010", "SET-017" }.SelectMany(id => Set(id).Recipe).ToList();
            var actives = components.Where(c => c.Kind == BuildEntryKind.ActiveSkill).Select(c => c.Id).Distinct().Count();
            var passives = components.Where(c => c.Kind == BuildEntryKind.PassiveItem).Select(c => c.Id).Distinct().Count();
            Assert.LessOrEqual(actives, 6);
            Assert.LessOrEqual(passives, 6);
        }

        [Test]
        public void StatAndSkillSets_ApplyApprovedValues()
        {
            var host = new FakeSetEffectHost();
            using (new SetEffectAbility(Set("SET-006"), host))
            {
                Assert.AreEqual(120f, host.Stats.MaxHealth, 1e-3f);
                Assert.AreEqual(0.6f, host.Stats.HealthRegenerationPerSecond, 1e-4f);
                Assert.AreEqual(1.25f, host.Stats.PotionDropMultiplier, 1e-4f);
            }
            Assert.AreEqual(100f, host.Stats.MaxHealth, 1e-3f, "Removing the set removes its buff.");
            using (new SetEffectAbility(Set("SET-001"), host))
            {
                var stone = host.SkillModifiers.Values.Single();
                Assert.AreEqual(0.6f, stone.ActiveSkillDamageMultiplierBonus, 1e-5f);
                Assert.AreEqual(0.25f, stone.EffectSizeMultiplierBonus, 1e-5f);
                Assert.AreEqual(0.35f, stone.OutgoingKnockbackBonus, 1e-5f);
            }
        }

        [Test]
        public void ColdSets_RegisterSlowedTargetBonusesAndOrbitAura_AndRemoveThem()
        {
            var host = new FakeSetEffectHost();
            using (new SetEffectAbility(Set("SET-004"), host))
            {
                var wide = host.SlowedBonuses.Values.Single();
                Assert.IsFalse(wide.skill.HasValue, "SET-004 knockback applies to all player skills.");
                Assert.AreEqual(0.5f, wide.bonus.KnockbackBonus, 1e-5f);
            }
            Assert.AreEqual(0, host.SlowedBonuses.Count);
            using (new SetEffectAbility(Set("SET-010"), host))
            {
                var orbit = host.SlowedBonuses.Values.Single();
                Assert.AreEqual("SKILL-003", orbit.skill.Value.ToString());
                Assert.AreEqual(0.4f, orbit.bonus.DamageBonus, 1e-5f);
                var aura = host.Auras.Values.Single();
                Assert.AreEqual("SKILL-003", aura.skill.ToString());
                Assert.AreEqual(0.15f, aura.fraction, 1e-5f);
                Assert.AreEqual(0.25f, aura.seconds, 1e-5f);
                Assert.AreEqual(0.1f, aura.refresh, 1e-5f);
            }
            Assert.AreEqual(0, host.Auras.Count);
        }

        [Test]
        public void FallingStar_FirstAttemptAfterFullCooldown_OptsIntoSizeAndRange_AndIgnoresActivations()
        {
            var host = new FakeSetEffectHost();
            using var ability = new SetEffectAbility(Set("SET-017"), host);
            for (var i = 0; i < 10; i++) host.Activate();
            ability.Tick(7.4f, true);
            Assert.AreEqual(0, host.Attacks, "No attack before the full 7.5 s after acquiring the set.");
            ability.Tick(0.1f, true);
            Assert.AreEqual(1, host.Attacks);
            Assert.IsTrue(host.LastAttackScalesWithSizeAndRange);
            ability.Tick(7.4f, true);
            Assert.AreEqual(1, host.Attacks);
            ability.Tick(0.1f, true);
            Assert.AreEqual(2, host.Attacks, "Fixed 7.5 s interval, independent of action speed and activations.");
        }

        [Test]
        public void SlowedTargetRequest_AddsKnockbackBonusBeforeResistance_AndDamageFactorOnce()
        {
            // Pulse wave L3 (KB 2.4) with belt L2 (+0.2), SET-004 wave transform (+0.35) and SET-004 slowed bonus (+0.5).
            var request = new CombatDamageRequest(default, 10f, new CombatControlProfile(2.4f, 0.12f), 1f, 0f, 1.55f)
                .WithSlowedTargetBonus(1f, 0.5f);
            var slowed = request.ResolveForSlowedTarget();
            Assert.AreEqual(2.05f, slowed.OutgoingKnockbackMultiplier, 1e-5f);
            var state = new CombatControlState();
            Assert.AreEqual(4.92f, state.Apply(slowed, 0f, true), 1e-4f);
            Assert.AreEqual(3.936f, new CombatControlState().Apply(slowed, 0.2f, true), 1e-4f);
            Assert.AreEqual(3.72f, new CombatControlState().Apply(request, 0f, true), 1e-4f, "Not slowed: no conditional bonus.");
            var orbit = new CombatDamageRequest(default, 20.3f).WithSlowedTargetBonus(new SlowedTargetBonus(0.4f, 0f).DamageFactor(1f), 0f);
            Assert.AreEqual(28.42f, orbit.ResolveForSlowedTarget().Amount, 1e-4f);
            Assert.AreEqual(1f, orbit.ResolveForSlowedTarget().SlowedTargetDamageFactor, "Bonus applies once, then clears.");
        }
    }
}

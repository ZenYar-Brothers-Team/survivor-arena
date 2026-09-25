using System.Linq;
using NUnit.Framework;

namespace Game.ActiveSkill.Tests
{
    /// <summary>F1-05: SET-017 attack template (baseline v1) — one telegraphed strike, not a draftable skill id.</summary>
    public sealed class ProductionSetAttackCatalogTests
    {
        [Test]
        public void FallingStarTemplate_IsOneTelegraphedStrike()
        {
            var template = ProductionSetAttackCatalog.Create().Single();
            Assert.AreEqual("SET-017-ATTACK", template.Id.ToString());
            var level = template.GetLevel(1);
            Assert.AreEqual(150f, level.BaseDamage);
            Assert.AreEqual(ActiveSkillTargetingMode.RandomEnemy, level.TargetingMode);
            Assert.AreEqual(8f, level.Targeting.Radius);
            var strike = (StrikeEffect)level.Waves.Single().Effects.Single();
            Assert.AreEqual(2.2f, strike.Radius, 1e-5f);
            Assert.AreEqual(0.65f, strike.TelegraphSeconds, 1e-5f);
            Assert.AreEqual(0.7f, strike.VerticalScale, 1e-5f, "DECISION-0058: flattened ground area.");
            Assert.AreEqual(1f, level.Waves[0].Controls.KnockbackDistance, 1e-5f);
            Assert.IsFalse(ProductionActiveSkillCatalog.Create().Any(s => s.Id == template.Id));
        }
    }
}

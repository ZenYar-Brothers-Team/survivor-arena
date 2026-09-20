using NUnit.Framework;

namespace Game.Combat.Tests
{
    public sealed class MeasuredHealthTests
    {
        [Test]
        public void OverkillUsesPostMitigationRequest_AndDeadTargetsDoNotInventOverkill()
        {
            using var health = new Health(new FixedHealthProfile(10f));
            var result = health.TakeDamageMeasured(100f);
            Assert.AreEqual(100f, result.Requested);
            Assert.AreEqual(100f, result.AfterMitigation);
            Assert.AreEqual(10f, result.Actual);
            Assert.AreEqual(90f, result.Overkill);
            Assert.AreEqual(0f, health.TakeDamageMeasured(100f).Overkill);
        }

        [Test]
        public void ReentrantHealingCannotChangeMeasuredDamage_OverhealIsNotOverkill()
        {
            using var health = new Health(new FixedHealthProfile(10f));
            health.Damaged += _ => health.Heal(100f);
            var result = health.TakeDamageMeasured(6f);
            Assert.AreEqual(10f, health.CurrentHealth);
            Assert.AreEqual(6f, result.Actual);
            var heal = health.HealMeasured(100f);
            Assert.AreEqual(100f, heal.AfterMitigation);
            Assert.AreEqual(0f, heal.Actual);
            Assert.AreEqual(0f, heal.Overkill);
            Assert.IsTrue(heal.IsHealing);
        }
    }
}

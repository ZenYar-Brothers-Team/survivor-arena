using NUnit.Framework;

namespace Game.Combat.Tests
{
    public class HealthTests
    {
        [Test]
        public void Damage_ReducesHealthAndReturnsAppliedAmount()
        {
            var health = new Health(new FixedHealthProfile(10f));

            var applied = health.TakeDamage(4f);

            Assert.AreEqual(4f, applied);
            Assert.AreEqual(6f, health.CurrentHealth);
            Assert.IsFalse(health.IsDead);
        }

        [Test]
        public void LethalDamage_ClampsAtZeroAndEmitsDeathOnce()
        {
            var health = new Health(new FixedHealthProfile(10f));
            var deaths = 0;
            health.Died += () => deaths++;

            var applied = health.TakeDamage(15f);
            var afterDeath = health.TakeDamage(1f);

            Assert.AreEqual(10f, applied);
            Assert.AreEqual(0f, afterDeath);
            Assert.AreEqual(0f, health.CurrentHealth);
            Assert.IsTrue(health.IsDead);
            Assert.AreEqual(1, deaths);
        }

        [Test]
        public void ZeroDamage_DoesNotEmitHealthChange()
        {
            var health = new Health(new FixedHealthProfile(10f));
            var changes = 0;
            var damageEvents = 0;
            health.HealthChanged += (_, _) => changes++;
            health.Damaged += _ => damageEvents++;

            health.TakeDamage(0f);

            Assert.AreEqual(10f, health.CurrentHealth);
            Assert.AreEqual(0, changes);
            Assert.AreEqual(0, damageEvents);
        }

        [Test]
        public void Damage_EmitsAppliedDamageOnly()
        {
            var health = new Health(new FixedHealthProfile(10f));
            var eventCount = 0;
            var lastAppliedDamage = 0f;
            health.Damaged += appliedDamage =>
            {
                eventCount++;
                lastAppliedDamage = appliedDamage;
            };

            health.TakeDamage(4f);
            health.Heal(2f);
            health.TakeDamage(100f);
            health.TakeDamage(1f);

            Assert.AreEqual(2, eventCount);
            Assert.AreEqual(8f, lastAppliedDamage, 0.0001f);
        }

        [Test]
        public void Heal_CapsAtMaxHealth()
        {
            var health = new Health(new FixedHealthProfile(10f));
            health.TakeDamage(6f);

            var applied = health.Heal(2f);
            var cappedHealing = health.Heal(100f);

            Assert.AreEqual(2f, applied, 0.0001f);
            Assert.AreEqual(4f, cappedHealing, 0.0001f);
            Assert.AreEqual(10f, health.CurrentHealth, 0.0001f);
        }
    }
}

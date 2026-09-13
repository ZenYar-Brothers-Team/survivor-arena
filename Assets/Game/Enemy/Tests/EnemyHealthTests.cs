using NUnit.Framework;

namespace Game.Enemy.Tests
{
    public class EnemyHealthTests
    {
        [Test]
        public void Damage_ReducesHealthAndReturnsAppliedAmount()
        {
            var health = new EnemyHealth(10f);

            var applied = health.TakeDamage(4f);

            Assert.AreEqual(4f, applied);
            Assert.AreEqual(6f, health.CurrentHealth);
            Assert.IsFalse(health.IsDead);
        }

        [Test]
        public void LethalDamage_ClampsAtZeroAndEmitsDeathOnce()
        {
            var health = new EnemyHealth(10f);
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
            var health = new EnemyHealth(10f);
            var changes = 0;
            health.HealthChanged += (_, _) => changes++;

            health.TakeDamage(0f);

            Assert.AreEqual(10f, health.CurrentHealth);
            Assert.AreEqual(0, changes);
        }
    }
}

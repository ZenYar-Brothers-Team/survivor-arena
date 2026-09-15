using Game.Combat;
using NUnit.Framework;

namespace Game.Character.Tests
{
    public class CharacterHealthIntegrationTests
    {
        [Test]
        public void Damage_UsesIncomingDamageMultiplier()
        {
            var stats = CreateStats();
            stats.SetModifier("armor", new CharacterStatModifier(incomingDamageReductionBonus: 0.25f));
            using (var health = new Health(stats))
            {
                var applied = health.TakeDamage(40f);

                Assert.AreEqual(30f, applied, 0.0001f);
                Assert.AreEqual(70f, health.CurrentHealth, 0.0001f);
            }
        }

        [Test]
        public void Healing_UsesRestorationMultiplierAndCapsAtMaxHealth()
        {
            var stats = CreateStats();
            stats.SetModifier("restoration", new CharacterStatModifier(healthRestorationMultiplierBonus: 0.5f));
            using (var health = new Health(stats))
            {
                health.TakeDamage(60f);

                var applied = health.Heal(20f);
                var cappedHealing = health.Heal(100f);

                Assert.AreEqual(30f, applied, 0.0001f);
                Assert.AreEqual(30f, cappedHealing, 0.0001f);
                Assert.AreEqual(100f, health.CurrentHealth, 0.0001f);
            }
        }

        [Test]
        public void Regeneration_OnlyRunsWhileRunIsActive()
        {
            var stats = CreateStats(regenerationPerSecond: 5f);
            stats.SetModifier("restoration", new CharacterStatModifier(healthRestorationMultiplierBonus: 0.5f));
            using (var health = new Health(stats))
            {
                health.TakeDamage(20f);

                var pausedHealing = health.Regenerate(2f, isRunning: false);
                var runningHealing = health.Regenerate(2f, isRunning: true);

                Assert.AreEqual(0f, pausedHealing);
                Assert.AreEqual(15f, runningHealing, 0.0001f);
                Assert.AreEqual(95f, health.CurrentHealth, 0.0001f);
            }
        }

        [Test]
        public void RemovingMaxHealthModifier_CapsCurrentHealth()
        {
            var stats = CreateStats();
            stats.SetModifier("health", new CharacterStatModifier(maxHealthMultiplierBonus: 0.5f));
            using (var health = new Health(stats))
            {
                health.Heal(50f);
                stats.RemoveModifier("health");

                Assert.AreEqual(100f, health.MaxHealth, 0.0001f);
                Assert.AreEqual(100f, health.CurrentHealth, 0.0001f);
            }
        }

        [Test]
        public void IncreasingMaxHealth_PreservesCurrentHealthRatio()
        {
            var stats = CreateStats();
            using (var health = new Health(stats))
            {
                health.TakeDamage(50f);
                stats.SetModifier("health", new CharacterStatModifier(maxHealthMultiplierBonus: 0.5f));

                Assert.AreEqual(150f, health.MaxHealth, 0.0001f);
                Assert.AreEqual(75f, health.CurrentHealth, 0.0001f);
            }
        }

        [Test]
        public void LethalDamage_EmitsDeathOnceAndPreventsHealing()
        {
            var stats = CreateStats();
            using (var health = new Health(stats))
            {
                var deaths = 0;
                health.Died += () => deaths++;

                health.TakeDamage(100f);
                health.TakeDamage(10f);
                var appliedHealing = health.Heal(100f);

                Assert.IsTrue(health.IsDead);
                Assert.AreEqual(0f, health.CurrentHealth);
                Assert.AreEqual(0f, appliedHealing);
                Assert.AreEqual(1, deaths);
            }
        }

        private static CharacterStats CreateStats(float regenerationPerSecond = 0f)
        {
            return new CharacterStats(new CharacterBaseStats(
                maxHealth: 100f,
                movementSpeed: 3f,
                healthRegenerationPerSecond: regenerationPerSecond));
        }
    }
}

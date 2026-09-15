using NUnit.Framework;

namespace Game.ActiveSkill.Tests
{
    public class ActiveSkillTimingTests
    {
        [Test]
        public void Cooldown_IsInitiallyReadyAndUsesConfiguredMultiplier()
        {
            var cooldown = new ActiveSkillCooldown();

            Assert.IsTrue(cooldown.IsReady);
            cooldown.Consume(2f, 0.5f);
            cooldown.Tick(0.99f, isRunning: true);
            Assert.IsFalse(cooldown.IsReady);
            cooldown.Tick(0.01f, isRunning: true);
            Assert.IsTrue(cooldown.IsReady);
        }

        [Test]
        public void Cooldown_DoesNotAdvanceWhilePaused()
        {
            var cooldown = new ActiveSkillCooldown();
            cooldown.Consume(1f, 1f);

            cooldown.Tick(10f, isRunning: false);

            Assert.AreEqual(1f, cooldown.RemainingSeconds);
        }

        [Test]
        public void ProjectileLifetime_DoesNotAdvanceWhilePaused()
        {
            var lifetime = new ProjectileLifetime(1f);

            Assert.IsFalse(lifetime.Tick(10f, isRunning: false));
            Assert.AreEqual(1f, lifetime.RemainingSeconds);
            Assert.IsTrue(lifetime.Tick(1f, isRunning: true));
        }
    }
}

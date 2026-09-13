using Game.Character;
using Game.Run;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    public class EnemyMovementAndContactTests
    {
        [Test]
        public void SeekVelocity_PointsAtTargetWithConfiguredSpeed()
        {
            var velocity = EnemyMovement.CalculateSeekVelocity(
                Vector2.zero,
                new Vector2(3f, 4f),
                movementSpeed: 2f,
                isSimulating: true);

            Assert.AreEqual(new Vector2(0.6f, 0.8f), velocity.normalized);
            Assert.AreEqual(2f, velocity.magnitude, 0.0001f);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void SeekVelocity_StopsWhenSimulationIsDisabledOrTargetReached(bool targetReached)
        {
            var target = targetReached ? Vector2.zero : Vector2.right;
            var isSimulating = targetReached;

            var velocity = EnemyMovement.CalculateSeekVelocity(Vector2.zero, target, 2f, isSimulating);

            Assert.AreEqual(Vector2.zero, velocity);
        }

        [Test]
        public void ContactDamage_DamagesPlayerOnlyDuringRunningRun()
        {
            var stats = new CharacterStats(new CharacterBaseStats(100f, 3f));
            using (var health = new CharacterHealth(stats))
            {
                var pausedDamage = EnemyContactDamage.Apply(10f, health, RunState.Paused);
                var runningDamage = EnemyContactDamage.Apply(10f, health, RunState.Running);
                var endedDamage = EnemyContactDamage.Apply(10f, health, RunState.Won);

                Assert.AreEqual(0f, pausedDamage);
                Assert.AreEqual(10f, runningDamage);
                Assert.AreEqual(0f, endedDamage);
                Assert.AreEqual(90f, health.CurrentHealth);
            }
        }

        [Test]
        public void ContactTimer_AppliesImmediateAndRepeatedHits()
        {
            var timer = new ContinuousContactTimer(0.5f);

            Assert.AreEqual(1, timer.BeginContact(isRunning: true));
            Assert.AreEqual(0, timer.Tick(0.49f, isRunning: true));
            Assert.AreEqual(1, timer.Tick(0.02f, isRunning: true));
            Assert.AreEqual(2, timer.Tick(1f, isRunning: true));
        }

        [Test]
        public void ContactTimer_PauseDoesNotAdvanceAndExitResetsContact()
        {
            var timer = new ContinuousContactTimer(1f);

            Assert.AreEqual(1, timer.BeginContact(isRunning: true));
            Assert.AreEqual(0, timer.Tick(0.75f, isRunning: true));
            Assert.AreEqual(0, timer.Tick(10f, isRunning: false));
            Assert.AreEqual(1, timer.Tick(0.25f, isRunning: true));

            timer.EndContact();

            Assert.AreEqual(0, timer.Tick(10f, isRunning: true));
        }

        [Test]
        public void ContactStartedDuringPause_HitsImmediatelyWhenRunResumes()
        {
            var timer = new ContinuousContactTimer(1f);

            Assert.AreEqual(0, timer.BeginContact(isRunning: false));
            Assert.AreEqual(1, timer.Tick(0f, isRunning: true));
        }

        [Test]
        public void SustainedContact_EventuallyKillsCharacter()
        {
            var timer = new ContinuousContactTimer(0.5f);
            var stats = new CharacterStats(new CharacterBaseStats(5f, 3f));
            using (var health = new CharacterHealth(stats))
            {
                ApplyHits(timer.BeginContact(isRunning: true), 1f, health);

                for (var i = 0; i < 4; i++)
                    ApplyHits(timer.Tick(0.5f, isRunning: true), 1f, health);

                Assert.IsTrue(health.IsDead);
                Assert.AreEqual(0f, health.CurrentHealth);
            }
        }

        private static void ApplyHits(int hitCount, float damage, CharacterHealth health)
        {
            for (var i = 0; i < hitCount; i++)
                EnemyContactDamage.Apply(damage, health, RunState.Running);
        }
    }
}

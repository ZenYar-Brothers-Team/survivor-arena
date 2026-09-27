using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    /// <summary>DECISION-0066 E1/E2/E6: follow-up volleys, the explosive fan and the wind-up movement factor.</summary>
    public sealed class EnemyAttackFollowUpTests
    {
        private static EnemyAttackProfile Ring(params EnemyAttackFollowUp[] followUps) =>
            new EnemyAttackProfile(EnemyProjectilePattern.Ring, 10f, 3f, 4f, 3f, 12, 360f, telegraphSeconds: 0.7f,
                cadence: EnemyAttackCadence.WindupStartToStart, followUps: followUps);

        private static List<(float time, EnemyShotCommand[] shots)> Run(EnemyAttackController controller, float seconds, Vector2 aim,
            bool running = true)
        {
            var fired = new List<(float, EnemyShotCommand[])>();
            const float dt = 0.02f;
            for (var t = dt; t <= seconds + 1e-4f; t += dt)
            {
                var shots = controller.Tick(dt, running, aim);
                if (shots.Length > 0) fired.Add((t, shots));
            }
            return fired;
        }

        private static EnemyShotCommand[] FireMain(EnemyAttackController controller, Vector2 aim)
        {
            for (var i = 0; i < 500; i++)
            {
                var shots = controller.Tick(0.02f, true, aim);
                if (shots.Length > 0) return shots;
            }
            throw new AssertionException("The main volley never fired.");
        }

        [Test]
        public void FollowUp_FiresSamePatternAfterDelay_RotatedFromTheMainShotDirection()
        {
            var controller = new EnemyAttackController(Ring(new EnemyAttackFollowUp(0.3f, 15f)), repeat: false);
            controller.RestartCycle();
            var main = FireMain(controller, Vector2.right);
            var more = Run(controller, 0.4f, Vector2.up); // aim changes after the main shot
            Assert.AreEqual(1, more.Count, "One follow-up.");
            Assert.AreEqual(12, more[0].shots.Length);
            Assert.AreEqual(0.3f, more[0].time, 0.025f);
            Assert.AreEqual(15f, Vector2.SignedAngle(main[0].Direction, more[0].shots[0].Direction), 1e-3f,
                "Rotated from the main volley, not from the new aim.");
            Assert.IsFalse(controller.FollowUpPending);
        }

        [Test]
        public void FollowUp_PendingKeepsTheCycleOpen_AndPauseFreezesIt()
        {
            var controller = new EnemyAttackController(Ring(new EnemyAttackFollowUp(0.4f, 0f), new EnemyAttackFollowUp(1.1f, 11.25f)),
                repeat: false);
            controller.RestartCycle();
            FireMain(controller, Vector2.right);
            Assert.IsTrue(controller.FollowUpPending);
            Assert.AreEqual(0, Run(controller, 5f, Vector2.right, running: false).Count, "Pause fires nothing.");
            Assert.IsTrue(controller.FollowUpPending);
            Assert.IsFalse(controller.CycleCompletesWithin(10f), "A step with pending follow-ups is not finished.");
            var more = Run(controller, 1.2f, Vector2.right);
            Assert.AreEqual(2, more.Count);
            Assert.IsFalse(controller.FollowUpPending);
        }

        [Test]
        public void ExplosiveFan_SpreadsSeveralExplosivesEvenly()
        {
            var fan = new EnemyAttackProfile(EnemyProjectilePattern.Explosive, 32f, 1f, 3.2f, 3f, 3, 40f, explosionRadius: 1.2f);
            var shots = EnemyProjectilePatternGenerator.Create(fan, Vector2.right);
            Assert.AreEqual(3, shots.Length);
            Assert.IsTrue(Array.TrueForAll(shots, s => s.IsExplosive));
            Assert.AreEqual(-20f, Vector2.SignedAngle(Vector2.right, shots[0].Direction), 1e-3f);
            Assert.AreEqual(0f, Vector2.SignedAngle(Vector2.right, shots[1].Direction), 1e-3f);
            Assert.AreEqual(20f, Vector2.SignedAngle(Vector2.right, shots[2].Direction), 1e-3f);
            var single = new EnemyAttackProfile(EnemyProjectilePattern.Explosive, 24f, 1f, 3f, 3f, explosionRadius: 1f);
            Assert.AreEqual(1, EnemyProjectilePatternGenerator.Create(single, Vector2.up).Length, "One explosive still flies straight.");
        }

        [Test]
        public void Profile_RejectsUnorderedFollowUps_BurstFollowUps_ExplosiveFanWithoutSpread_AndBadWindupFactor()
        {
            Assert.Throws<ArgumentException>(() => Ring(new EnemyAttackFollowUp(0.5f, 0f), new EnemyAttackFollowUp(0.4f, 0f)));
            Assert.Throws<ArgumentException>(() => new EnemyAttackProfile(EnemyProjectilePattern.Burst, 1f, 1f, 1f, 1f, 3,
                followUps: new[] { new EnemyAttackFollowUp(0.2f, 0f) }));
            Assert.Throws<ArgumentException>(() => new EnemyAttackProfile(EnemyProjectilePattern.Explosive, 1f, 1f, 1f, 1f, 3,
                explosionRadius: 1f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyAttackFollowUp(0f, 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyAttackProfile(EnemyProjectilePattern.Single, 1f, 1f, 1f, 1f,
                windupMovementMultiplier: 0f));
            Assert.Throws<ArgumentOutOfRangeException>(() => new EnemyAttackProfile(EnemyProjectilePattern.Single, 1f, 1f, 1f, 1f,
                windupMovementMultiplier: 1.5f));
            Assert.AreEqual(1f, new EnemyAttackProfile(EnemyProjectilePattern.Single, 1f, 1f, 1f, 1f).WindupMovementMultiplier,
                "Neutral default leaves movement unchanged.");
        }
    }
}

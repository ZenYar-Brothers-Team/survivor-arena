using System;
using System.Collections.Generic;
using Game.Combat;
using Game.Enemy.Json;
using NUnit.Framework;
using UnityEngine;

namespace Game.Enemy.Tests
{
    /// <summary>DECISION-0059: boss teleport-slam timing, landing and hit area.</summary>
    public sealed class BossTeleportControllerTests
    {
        private const float Dt = 0.02f;

        private static BossTeleportProfile Profile(float farDistance = 5f, float farSeconds = 5f, float landing = 2.2f,
            float telegraph = 0.6f, float radius = 2.6f) =>
            new BossTeleportProfile(farDistance, farSeconds, landing, telegraph, radius, 20f,
                new CombatControlProfile(0.5f, 0.12f), 0.45f, Color.red, Color.yellow);

        private static BossTeleportSignal Advance(BossTeleportController controller, float seconds, Vector2 boss, Vector2 player,
            bool running = true)
        {
            var last = BossTeleportSignal.None;
            for (var t = 0f; t < seconds - 1e-4f; t += Dt)
            {
                var signal = controller.Tick(Dt, running, boss, player);
                if (signal != BossTeleportSignal.None) last = signal;
            }
            return last;
        }

        [Test]
        public void FarForFarSeconds_TelegraphsLandingOnCircleAroundPlayer_ThenImpacts()
        {
            var controller = new BossTeleportController(Profile(), new System.Random(7));
            var boss = new Vector2(20f, 1f);
            var player = new Vector2(1f, 1f);
            Assert.AreEqual(BossTeleportSignal.None, Advance(controller, 4.9f, boss, player));
            Assert.AreEqual(BossTeleportPhase.Tracking, controller.Phase);
            Assert.AreEqual(BossTeleportSignal.TelegraphStarted, Advance(controller, 0.2f, boss, player));
            Assert.AreEqual(BossTeleportPhase.Telegraphing, controller.Phase);
            Assert.AreEqual(2.2f, (controller.Landing - player).magnitude, 1e-4f);
            Assert.AreEqual(BossTeleportSignal.Impact, Advance(controller, 0.62f, boss, player));
            Assert.AreEqual(BossTeleportPhase.Tracking, controller.Phase);
            Assert.Less(controller.FarElapsed, 0.1f, "The far timer restarts after the slam.");
        }

        [Test]
        public void ComingWithinFarDistance_ResetsTheFarTimer()
        {
            var controller = new BossTeleportController(Profile(), new System.Random(7));
            var boss = Vector2.zero;
            Advance(controller, 4f, boss, new Vector2(6f, 0f));
            Advance(controller, Dt, boss, new Vector2(4f, 0f));
            Assert.AreEqual(0f, controller.FarElapsed);
            Assert.AreEqual(BossTeleportSignal.None, Advance(controller, 4.9f, boss, new Vector2(6f, 0f)));
            Assert.AreEqual(BossTeleportPhase.Tracking, controller.Phase);
        }

        [Test]
        public void Pause_AdvancesNeitherFarTimerNorTelegraph()
        {
            var controller = new BossTeleportController(Profile(), new System.Random(7));
            var boss = new Vector2(10f, 0f);
            Advance(controller, 3f, boss, Vector2.zero);
            var far = controller.FarElapsed;
            Assert.AreEqual(BossTeleportSignal.None, Advance(controller, 10f, boss, Vector2.zero, running: false));
            Assert.AreEqual(far, controller.FarElapsed);
            Advance(controller, 2.1f, boss, Vector2.zero);
            Assert.AreEqual(BossTeleportPhase.Telegraphing, controller.Phase);
            var remaining = controller.TelegraphRemaining;
            Advance(controller, 10f, boss, Vector2.zero, running: false);
            Assert.AreEqual(remaining, controller.TelegraphRemaining);
        }

        [Test]
        public void StartedTelegraph_LandsEvenIfPlayerComesClose_AndHitsOnlyInsideRadius()
        {
            var controller = new BossTeleportController(Profile(), new System.Random(7));
            Advance(controller, 5.02f, new Vector2(0f, -9f), Vector2.zero);
            Assert.AreEqual(BossTeleportPhase.Telegraphing, controller.Phase);
            Assert.AreEqual(BossTeleportSignal.Impact, Advance(controller, 0.62f, Vector2.zero, Vector2.zero));
            var away = -controller.Landing.normalized;
            Assert.IsTrue(controller.Hits(Vector2.zero), "A player who stayed put is caught.");
            Assert.IsTrue(controller.Hits(away * 0.39f));
            Assert.IsFalse(controller.Hits(away * 0.5f), "Stepping away from the marker dodges the slam.");
        }

        [Test]
        public void Landing_IsRandomOnCircle_IndependentOfBossSide()
        {
            var controller = new BossTeleportController(Profile(), new System.Random(11));
            var boss = new Vector2(20f, 0f);
            var player = Vector2.zero;
            var quadrants = new HashSet<int>();
            for (var i = 0; i < 40; i++)
            {
                Advance(controller, 5.1f, boss, player);
                Assert.AreEqual(BossTeleportPhase.Telegraphing, controller.Phase);
                Assert.AreEqual(2.2f, controller.Landing.magnitude, 1e-4f);
                quadrants.Add((controller.Landing.x >= 0f ? 0 : 1) + (controller.Landing.y >= 0f ? 0 : 2));
                Advance(controller, 0.62f, boss, player);
                Assert.AreEqual(BossTeleportPhase.Tracking, controller.Phase);
                Advance(controller, Dt, boss, new Vector2(19f, 0f));
            }
            Assert.AreEqual(4, quadrants.Count, "Landings are spread around the player, not only toward the boss.");
        }

        [Test]
        public void Profile_RejectsLandingOutsideFarDistance_AndMissingData()
        {
            Assert.Throws<ArgumentException>(() => Profile(farDistance: 2f, landing: 2.2f));
            Assert.Throws<ArgumentOutOfRangeException>(() => Profile(telegraph: 0f));
            Assert.IsNull(BossTeleportProfile.FromData(null, "Boss X"));
            Assert.Throws<InvalidOperationException>(() => BossTeleportProfile.FromData(new BossTeleportData
            {
                FarDistance = 5, FarSeconds = 5, LandingDistance = 2, TelegraphSeconds = .5f, ImpactRadius = 2,
                ImpactDamage = 10, ImpactEffectSeconds = .4f, TelegraphColor = new[] { 1f, 0f, 0f, 1f },
                ImpactColor = new[] { 1f, 1f, 0f, 1f }
            }, "Boss X"), "Impact controls must be explicit.");
            Assert.Throws<InvalidOperationException>(() => BossTeleportProfile.FromData(new BossTeleportData
            {
                FarDistance = 5, FarSeconds = 5, LandingDistance = 2, TelegraphSeconds = .5f, ImpactRadius = 2,
                ImpactDamage = 10, ImpactEffectSeconds = .4f, ImpactControls = new CombatControlData { KnockbackDistance = 0 },
                TelegraphColor = new[] { 1f, 0f, 0f }, ImpactColor = new[] { 1f, 1f, 0f, 1f }
            }, "Boss X"));
        }
    }
}

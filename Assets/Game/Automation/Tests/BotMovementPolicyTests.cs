using NUnit.Framework;
using UnityEngine;

namespace Game.Automation.Tests
{
    public sealed class BotMovementPolicyTests
    {
        private static MovementPolicyData Settings(float stuckSeconds = 3f) => new MovementPolicyData
        {
            Id = "safePickup", Version = 1, DecisionIntervalSeconds = 0.2f, ObservationRadius = 12f,
            PredictionSeconds = 0.5f, ObstaclePadding = 0.15f, StuckSeconds = stuckSeconds
        };

        private static BotObservation Observe(Vector2 position, BotThreat[] threats = null, BotPickup[] pickups = null,
            BotObstacle[] obstacles = null, BotBeam[] beams = null, bool complete = true) =>
            new BotObservation(position, 3f, 0.3f, Rect.MinMaxRect(-10, -10, 10, 10),
                threats ?? new BotThreat[0], pickups ?? new BotPickup[0], obstacles ?? new BotObstacle[0],
                beams ?? new BotBeam[0], complete);

        [Test]
        public void Decide_VisibleXp_ApproachesWithoutGameplayRandomness()
        {
            var policy = new BotMovementPolicy(Settings());
            var observation = Observe(Vector2.zero, pickups: new[] { new BotPickup(new Vector2(3, 0), 1f) });
            Assert.AreEqual(Vector2.right, policy.Decide(observation, 0.2f).Direction);
        }

        [Test]
        public void Decide_ObstacleBetweenPlayerAndXp_ChoosesDetour()
        {
            var policy = new BotMovementPolicy(Settings());
            var obstacle = new BotObstacle(Rect.MinMaxRect(0.5f, -0.5f, 2f, 0.5f));
            var decision = policy.Decide(Observe(Vector2.zero,
                pickups: new[] { new BotPickup(new Vector2(3, 0), 1f) }, obstacles: new[] { obstacle }), 0.2f);
            Assert.AreNotEqual(Vector2.right, decision.Direction);
            Assert.AreNotEqual(Vector2.zero, decision.Direction);
            Assert.Greater(Mathf.Abs(decision.Direction.y), 0f);
        }

        [Test]
        public void Decide_IncomingProjectile_AvoidsDirectPickupPath()
        {
            var policy = new BotMovementPolicy(Settings());
            var projectile = new BotThreat(new Vector2(2, 0), new Vector2(-2, 0), 0.25f, 10f);
            var decision = policy.Decide(Observe(Vector2.zero,
                threats: new[] { projectile }, pickups: new[] { new BotPickup(new Vector2(3, 0), 1f) }), 0.2f);
            Assert.AreNotEqual(Vector2.right, decision.Direction);
        }

        [Test]
        public void Decide_BlockedObservation_StopsAndReportsCoverage()
        {
            var policy = new BotMovementPolicy(Settings());
            var decision = policy.Decide(Observe(Vector2.zero, complete: false), 0.2f);
            Assert.AreEqual(Vector2.zero, decision.Direction);
            Assert.IsTrue(decision.CoverageIncomplete);
        }

        [Test]
        public void Decide_StillBlockedAfterRecovery_ReportsStuck()
        {
            var policy = new BotMovementPolicy(Settings(0.5f));
            var observation = Observe(Vector2.zero, pickups: new[] { new BotPickup(new Vector2(3, 0), 1f) });
            BotMovementDecision decision = default;
            for (var i = 0; i < 18; i++) decision = policy.Decide(observation, 0.5f);
            Assert.IsTrue(decision.Stuck);
        }
    }
}

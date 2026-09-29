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

        private static MovementPolicyData OrbitSettings(float arcOffset = 6f)
        {
            var settings = Settings();
            settings.Id = "orbitExperience";
            settings.ArcOffsetWorldUnits = arcOffset;
            return settings;
        }

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
        public void Decide_ExperienceFocusedProfile_PrefersXpOverWorldPickup()
        {
            var observation = Observe(Vector2.zero, pickups: new[]
            {
                new BotPickup(new Vector2(3, 0), 1f),
                new BotPickup(new Vector2(-3, 0), 2f, isExperience: false)
            });
            var safe = new BotMovementPolicy(Settings());
            var focusedSettings = Settings();
            focusedSettings.Id = "experienceFocused";
            var focused = new BotMovementPolicy(focusedSettings);
            Assert.AreEqual(Vector2.left, safe.Decide(observation, 0.2f).Direction);
            Assert.AreEqual(Vector2.right, focused.Decide(observation, 0.2f).Direction);
        }

        [Test]
        public void Decide_ExperienceFocusedProfile_AcceptsModerateThreatButAvoidsProjectile()
        {
            var settings = Settings();
            settings.Id = "experienceFocused";
            var pickup = new[] { new BotPickup(new Vector2(3, 0), 1f) };
            var moderate = Observe(Vector2.zero,
                threats: new[] { new BotThreat(new Vector2(2, 1.5f), Vector2.zero, 0.5f, 3f) }, pickups: pickup);
            Assert.Greater(new BotMovementPolicy(settings).Decide(moderate, 0.2f).Direction.x, 0f);
            var projectile = Observe(Vector2.zero,
                threats: new[] { new BotThreat(new Vector2(2, 0), new Vector2(-2, 0), 0.25f, 10f) }, pickups: pickup);
            Assert.AreNotEqual(Vector2.right, new BotMovementPolicy(settings).Decide(projectile, 0.2f).Direction);
        }

        [Test]
        public void Decide_OrbitExperience_BlockedDirectRouteTakesWideUpperArcThenReturnsToXp()
        {
            var policy = new BotMovementPolicy(OrbitSettings());
            var threat = new[] { new BotThreat(new Vector2(4, 0), Vector2.zero, 0.5f, 3f) };
            var xp = new[] { new BotPickup(new Vector2(8, 0), 1f) };
            var first = policy.Decide(Observe(Vector2.zero, threats: threat, pickups: xp), 0.2f);
            Assert.Greater(first.Direction.y, 0f);
            var afterWaypoint = policy.Decide(Observe(new Vector2(4, 5.5f), threats: threat, pickups: xp), 0.2f);
            Assert.Greater(afterWaypoint.Direction.x, 0f);
            Assert.Less(afterWaypoint.Direction.y, 0f);
        }

        [Test]
        public void Decide_OrbitExperience_ChoosesSaferSideAndKeepsTarget()
        {
            var policy = new BotMovementPolicy(OrbitSettings());
            var threats = new[]
            {
                new BotThreat(new Vector2(4, 0), Vector2.zero, 0.5f, 3f),
                new BotThreat(new Vector2(4, 5), Vector2.zero, 0.5f, 3f)
            };
            var xp = new[] { new BotPickup(new Vector2(8, 0), 1f) };
            Assert.Less(policy.Decide(Observe(Vector2.zero, threats: threats, pickups: xp), 0.2f).Direction.y, 0f);
            var addedCloserXp = new[] { xp[0], new BotPickup(new Vector2(-1, 0), 1f) };
            var next = policy.Decide(Observe(new Vector2(1, -1), threats: threats, pickups: addedCloserXp), 0.2f);
            Assert.Greater(next.Direction.x, 0f);
        }

        [Test]
        public void Decide_OrbitExperience_NoLegalArcDoesNotRushXp()
        {
            var policy = new BotMovementPolicy(OrbitSettings(10f));
            var observation = Observe(Vector2.zero,
                threats: new[] { new BotThreat(new Vector2(4, 0), Vector2.zero, 0.5f, 3f) },
                pickups: new[] { new BotPickup(new Vector2(8, 0), 1f) });
            Assert.AreNotEqual(Vector2.right, policy.Decide(observation, 0.2f).Direction);
        }

        [Test]
        public void Decide_OrbitExperience_ObstacleOnUpperArcChoosesLowerArc()
        {
            var policy = new BotMovementPolicy(OrbitSettings());
            var observation = Observe(Vector2.zero,
                threats: new[] { new BotThreat(new Vector2(4, 0), Vector2.zero, 0.5f, 3f) },
                pickups: new[] { new BotPickup(new Vector2(8, 0), 1f) },
                obstacles: new[] { new BotObstacle(Rect.MinMaxRect(2, 2, 5, 7)) });
            Assert.Less(policy.Decide(observation, 0.2f).Direction.y, 0f);
        }

        [Test]
        public void Decide_OrbitExperience_DisappearedXpClearsWaypoint()
        {
            var policy = new BotMovementPolicy(OrbitSettings());
            var threat = new[] { new BotThreat(new Vector2(4, 0), Vector2.zero, 0.5f, 3f) };
            policy.Decide(Observe(Vector2.zero, threats: threat,
                pickups: new[] { new BotPickup(new Vector2(8, 0), 1f) }), 0.2f);
            Assert.AreEqual(Vector2.zero, policy.Decide(Observe(Vector2.zero), 0.2f).Direction);
        }

        [Test]
        public void Decide_OrbitExperience_NearestXpInsideThreatChoosesOtherVisibleXp()
        {
            var policy = new BotMovementPolicy(OrbitSettings());
            var observation = Observe(Vector2.zero,
                threats: new[] { new BotThreat(new Vector2(3, 0), Vector2.zero, 0.8f, 3f) },
                pickups: new[]
                {
                    new BotPickup(new Vector2(3, 0), 1f),
                    new BotPickup(new Vector2(0, 6), 1f)
                });
            Assert.Greater(policy.Decide(observation, 0.2f).Direction.y, 0f);
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

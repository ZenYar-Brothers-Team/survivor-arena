using System;
using System.Collections.Generic;
using System.Diagnostics;
using Game.Enemy;
using NUnit.Framework;
using UnityEngine;

namespace Game.Automation.Tests
{
    public sealed class BotTrajectoryPlannerTests
    {
        private static MovementPolicyData Settings() => new MovementPolicyData
        {
            Id = "trajectorySearch", Version = 1, DecisionIntervalSeconds = 0.2f,
            ObservationRadius = 12f, PredictionSeconds = 0.5f, ObstaclePadding = 0.1f, StuckSeconds = 3f,
            Trajectory = new TrajectoryPolicyData
            {
                HorizonSeconds = 8f, StepSeconds = 0.2f, CandidateCount = 96,
                ContactPenalty = 60f, Clearance = 0.25f
            }
        };

        private static BotObservation Observe(Vector2 player, IReadOnlyList<BotThreat> threats,
            IReadOnlyList<BotPickup> pickups, IReadOnlyList<BotObstacle> obstacles = null, bool complete = true) =>
            new BotObservation(player, 3f, 0.3f, Rect.MinMaxRect(-10, -10, 10, 10), threats, pickups,
                obstacles ?? Array.Empty<BotObstacle>(), Array.Empty<BotBeam>(), complete, pickupRadius: 0.5f);

        [Test]
        public void Forecast_SeekRespondsToFuturePlayerDirection()
        {
            var seek = new BotThreat(Vector2.zero, Vector2.right, 0.35f, 3f, true, BotThreatMotion.Seek, 1f);
            var next = BotThreatForecast.Advance(seek, Vector2.zero, Vector2.up * 3f, 1f);
            Assert.AreEqual(Vector2.up, next);
            Assert.AreEqual(Vector2.zero, seek.Position, "Forecast must not change observed state.");
        }

        [Test]
        public void Forecast_KeepDistanceAtPreferredRangeHolds()
        {
            var ranged = new BotThreat(Vector2.zero, Vector2.right, 0.35f, 3f, true,
                BotThreatMotion.KeepDistance, 1f, preferredDistance: 3f, distanceTolerance: 0.2f);
            Assert.AreEqual(Vector2.zero, BotThreatForecast.Advance(ranged, Vector2.zero, Vector2.right * 3f, 1f));
            Assert.Less(BotThreatForecast.Advance(ranged, Vector2.zero, Vector2.right, 1f).x, 0f);
        }

        [TestCase(0f)]
        [TestCase(90f)]
        [TestCase(180f)]
        public void ClosedLoop_XpBehindPursuingCrowd_CollectsWithoutContact(float rotation)
        {
            var enemies = new Vector2[8];
            for (var i = 0; i < enemies.Length; i++)
                enemies[i] = Rotate(new Vector2(2.5f + i % 3 * 0.4f, (i / 3 - 1) * 0.4f), rotation);
            RunScenario(enemies, Rotate(new Vector2(6, 0), rotation), Array.Empty<BotObstacle>());
        }

        [Test]
        public void ClosedLoop_DenseFasterCrowd_CollectsWithoutContact()
        {
            var enemies = new Vector2[25];
            for (var i = 0; i < enemies.Length; i++)
                enemies[i] = new Vector2(1.6f + i % 5 * 0.4f, (i / 5 - 2) * 0.65f);
            RunScenario(enemies, new Vector2(6, 0), Array.Empty<BotObstacle>(), enemySpeed: 1.8f);
        }

        [Test]
        public void ClosedLoop_XpBehindWall_UsesReachableDetour()
        {
            RunScenario(Array.Empty<Vector2>(), new Vector2(6, 0),
                new[] { new BotObstacle(Rect.MinMaxRect(2, -1.5f, 4, 1.5f)) });
        }

        private static void RunScenario(Vector2[] enemies, Vector2 xpPosition, BotObstacle[] obstacles, float enemySpeed = 1.2f)
        {
            // Independent test environment uses the real Seek controller, not the planner's forecast.
            var controllers = new EnemyMovementController[enemies.Length];
            for (var i = 0; i < controllers.Length; i++) controllers[i] = new EnemyMovementController(EnemyMovementProfile.Seek);
            var player = Vector2.zero;
            var policy = new BotMovementPolicy(Settings());
            var pickups = new List<BotPickup> { new BotPickup(xpPosition, 1f) };
            var direction = Vector2.zero;
            var collectedAt = -1f;
            var contacts = 0;
            var minClearance = float.PositiveInfinity;
            for (var tick = 0; tick < 160; tick++)
            {
                var time = tick * 0.1f;
                if (tick % 2 == 0)
                {
                    var threats = new BotThreat[enemies.Length];
                    for (var i = 0; i < threats.Length; i++)
                        threats[i] = new BotThreat(enemies[i], Vector2.zero, 0.35f, 3f, true, BotThreatMotion.Seek, enemySpeed);
                    direction = policy.Decide(Observe(player, threats, pickups, obstacles), 0.2f).Direction;
                }
                var next = player + direction * 0.3f;
                Assert.LessOrEqual(Mathf.Abs(next.x), 9.7f);
                Assert.LessOrEqual(Mathf.Abs(next.y), 9.7f);
                foreach (var obstacle in obstacles)
                {
                    var b = obstacle.Bounds;
                    b.xMin -= 0.3f; b.xMax += 0.3f; b.yMin -= 0.3f; b.yMax += 0.3f;
                    Assert.IsFalse(b.Contains(next), "Player crossed an obstacle.");
                }
                for (var i = 0; i < enemies.Length; i++)
                {
                    enemies[i] += controllers[i].Tick(enemies[i], player, enemySpeed, 0.1f, true).Velocity * 0.1f;
                    var clearance = Vector2.Distance(next, enemies[i]) - 0.65f;
                    minClearance = Mathf.Min(minClearance, clearance);
                    if (clearance < 0f) contacts++;
                }
                player = next;
                if (collectedAt < 0f && Vector2.Distance(player, xpPosition) <= 0.5f)
                {
                    collectedAt = time;
                    pickups.Clear();
                }
                if (collectedAt >= 0f && time >= collectedAt + 2f) break;
            }
            TestContext.WriteLine($"collectedAt={collectedAt:F2}s contacts={contacts} minClearance={minClearance:F3}");
            Assert.GreaterOrEqual(collectedAt, 0f, "Did not complete the XP collection maneuver within 16 seconds.");
            Assert.AreEqual(0, contacts, "The predicted detour crossed pursuing enemies.");
        }

        [Test]
        public void Decide_CrossingProjectile_SelectsSafeWholeTrajectory()
        {
            var policy = new BotMovementPolicy(Settings());
            var projectile = new BotThreat(new Vector2(3, 3), new Vector2(0, -3), 0.2f, 10f);
            policy.Decide(Observe(Vector2.zero, new[] { projectile }, new[] { new BotPickup(new Vector2(6, 0), 1f) }), 0.2f);
            Assert.Greater(policy.CurrentTrajectoryPlan.PredictedXp, 0f);
            Assert.Less(policy.CurrentTrajectoryPlan.ContactRiskSeconds, 0.01f);
            Assert.Greater(Mathf.Abs(policy.CurrentTrajectoryPlan.Direction.y), 0.01f);
        }

        [Test]
        public void Decide_NearbyXp_AllCandidatesUseTheFullHorizon()
        {
            var policy = new BotMovementPolicy(Settings());
            policy.Decide(Observe(Vector2.zero, Array.Empty<BotThreat>(),
                new[] { new BotPickup(new Vector2(1, 0), 1f) }), 0.2f);
            Assert.Greater(policy.CurrentTrajectoryPlan.PredictedXp, 0f);
            Assert.AreEqual(41, policy.CurrentTrajectoryPlan.Path.Count,
                "XP routes must include the post-pickup future, like escape routes.");
        }

        [Test]
        public void ClosedLoop_NoXpWithPursuers_UsesCurvedSearchAndSurvives()
        {
            var policy = new BotMovementPolicy(Settings());
            var enemies = new[] { new Vector2(3, 0), new Vector2(3, 1), new Vector2(3, -1) };
            var controllers = new[] { new EnemyMovementController(EnemyMovementProfile.Seek),
                new EnemyMovementController(EnemyMovementProfile.Seek), new EnemyMovementController(EnemyMovementProfile.Seek) };
            var player = Vector2.zero;
            var direction = Vector2.zero;
            var contacts = 0;
            for (var tick = 0; tick < 300; tick++)
            {
                if (tick % 2 == 0)
                {
                    var threats = new BotThreat[enemies.Length];
                    for (var i = 0; i < threats.Length; i++)
                        threats[i] = new BotThreat(enemies[i], Vector2.zero, 0.35f, 3f, true, BotThreatMotion.Seek, 1.2f);
                    direction = policy.Decide(Observe(player, threats, Array.Empty<BotPickup>()), 0.2f).Direction;
                    Assert.AreEqual(96, policy.CurrentTrajectoryPlan.EvaluatedCandidates);
                }
                for (var i = 0; i < enemies.Length; i++)
                    enemies[i] += controllers[i].Tick(enemies[i], player, 1.2f, 0.1f, true).Velocity * 0.1f;
                player += direction * 0.3f;
                foreach (var enemy in enemies)
                    if (Vector2.Distance(player, enemy) < 0.65f) contacts++;
                Assert.LessOrEqual(Mathf.Abs(player.x), 9.7f);
                Assert.LessOrEqual(Mathf.Abs(player.y), 9.7f);
            }
            Assert.AreEqual(0, contacts);
        }

        [Test]
        public void Decide_ExpiredXpDoesNotCountAndIncompleteCoverageClearsPlan()
        {
            var policy = new BotMovementPolicy(Settings());
            var pickups = new[] { new BotPickup(new Vector2(6, 0), 1f, remainingSeconds: 0.1f) };
            policy.Decide(Observe(Vector2.zero, Array.Empty<BotThreat>(), pickups), 0.2f);
            Assert.AreEqual(0f, policy.CurrentTrajectoryPlan.PredictedXp);
            var decision = policy.Decide(Observe(Vector2.zero, Array.Empty<BotThreat>(), pickups, complete: false), 0.2f);
            Assert.IsTrue(decision.CoverageIncomplete);
            Assert.IsNull(policy.CurrentTrajectoryPlan);
            Assert.AreEqual(Vector2.zero, decision.Direction);
        }

        [Test]
        public void Decide_SameObservationAndPolicySeed_RepeatsWithinSearchBudget()
        {
            var observation = Observe(Vector2.zero, Array.Empty<BotThreat>(), new[] { new BotPickup(new Vector2(6, 0), 1f) });
            var first = new BotMovementPolicy(Settings());
            var second = new BotMovementPolicy(Settings());
            Assert.AreEqual(first.Decide(observation, 0.2f).Direction, second.Decide(observation, 0.2f).Direction);
            Assert.AreEqual(first.CurrentTrajectoryPlan.Score, second.CurrentTrajectoryPlan.Score);
            Assert.AreEqual(96, first.CurrentTrajectoryPlan.EvaluatedCandidates);
            Assert.AreEqual(new Vector2(6, 0), observation.Pickups[0].Position);
        }

        [Test]
        public void Decide_TwoHundredThreats_CompletesBoundedSearchUnderGenerousBudget()
        {
            var threats = new BotThreat[200];
            for (var i = 0; i < threats.Length; i++)
                threats[i] = new BotThreat(new Vector2(5 + i % 10 * 0.3f, -3 + i / 10 * 0.3f),
                    Vector2.zero, 0.35f, 3f, true, BotThreatMotion.Seek, 1.2f);
            var observation = Observe(Vector2.zero, threats, new[] { new BotPickup(new Vector2(6, 0), 1f) });
            var policy = new BotMovementPolicy(Settings());
            policy.Decide(observation, 0.2f);
            var timer = Stopwatch.StartNew();
            policy.Decide(observation, 0.2f);
            Assert.Less(timer.Elapsed.TotalMilliseconds, 500, "Bounded prototype search exceeded 500 ms at 200 threats.");
            Assert.AreEqual(96, policy.CurrentTrajectoryPlan.EvaluatedCandidates);
        }

        private static Vector2 Rotate(Vector2 point, float degrees)
        {
            var radians = degrees * Mathf.Deg2Rad;
            return new Vector2(point.x * Mathf.Cos(radians) - point.y * Mathf.Sin(radians),
                point.x * Mathf.Sin(radians) + point.y * Mathf.Cos(radians));
        }
    }
}

using NUnit.Framework;
using UnityEngine;

namespace Game.MLAgents.Tests
{
    public sealed class MicroArenaRewardCalculatorTests
    {
        [Test]
        public void StepReward_ClosingDistance_IsPositiveNetOfTimePenalty()
        {
            var reward = MicroArenaRewardCalculator.StepReward(previousDistanceToXp: 2f, currentDistanceToXp: 1.9f);

            Assert.AreEqual(-0.001f + (0.3f * 0.1f), reward, 0.0001f);
        }

        [Test]
        public void StepReward_MovingAway_IsNegative()
        {
            var reward = MicroArenaRewardCalculator.StepReward(previousDistanceToXp: 1f, currentDistanceToXp: 1.2f);

            Assert.Less(reward, 0f);
        }

        [Test]
        public void DetermineOutcome_PlayerWithinXpContactRadius_ReturnsCollected()
        {
            var config = MicroArenaConfig.Default(1);
            var player = Vector2.zero;
            var xp = new Vector2(config.PlayerRadius + config.XpRadius - 0.01f, 0f);
            var threat = new Vector2(100f, 100f);

            var outcome = MicroArenaRewardCalculator.DetermineOutcome(player, xp, threat, config, elapsedSeconds: 0.1f);

            Assert.AreEqual(MicroArenaOutcome.Collected, outcome);
        }

        [Test]
        public void DetermineOutcome_CollectAndCollisionBothTrue_PrefersCollected()
        {
            var config = MicroArenaConfig.Default(1);
            var player = Vector2.zero;
            var xp = new Vector2(config.PlayerRadius + config.XpRadius - 0.01f, 0f);
            var threat = new Vector2(config.PlayerRadius + config.ThreatRadius - 0.01f, 0f);

            var outcome = MicroArenaRewardCalculator.DetermineOutcome(player, xp, threat, config, elapsedSeconds: 0.1f);

            Assert.AreEqual(MicroArenaOutcome.Collected, outcome);
        }

        [Test]
        public void DetermineOutcome_PlayerWithinThreatContactRadius_ReturnsCollision()
        {
            var config = MicroArenaConfig.Default(1);
            var player = Vector2.zero;
            var xp = new Vector2(100f, 100f);
            var threat = new Vector2(config.PlayerRadius + config.ThreatRadius - 0.01f, 0f);

            var outcome = MicroArenaRewardCalculator.DetermineOutcome(player, xp, threat, config, elapsedSeconds: 0.1f);

            Assert.AreEqual(MicroArenaOutcome.Collision, outcome);
        }

        [Test]
        public void DetermineOutcome_TimeLimitReachedWithoutContact_ReturnsTimeout()
        {
            var config = MicroArenaConfig.Default(1);
            var player = Vector2.zero;
            var xp = new Vector2(100f, 100f);
            var threat = new Vector2(100f, -100f);

            var outcome = MicroArenaRewardCalculator.DetermineOutcome(
                player, xp, threat, config, elapsedSeconds: config.TimeLimitSeconds);

            Assert.AreEqual(MicroArenaOutcome.Timeout, outcome);
        }

        [Test]
        public void DetermineOutcome_NoContactAndTimeRemaining_ReturnsNone()
        {
            var config = MicroArenaConfig.Default(1);
            var player = Vector2.zero;
            var xp = new Vector2(100f, 100f);
            var threat = new Vector2(100f, -100f);

            var outcome = MicroArenaRewardCalculator.DetermineOutcome(
                player, xp, threat, config, elapsedSeconds: config.TimeLimitSeconds - 0.01f);

            Assert.AreEqual(MicroArenaOutcome.None, outcome);
        }

        [TestCase(MicroArenaOutcome.Collected, 5f)]
        [TestCase(MicroArenaOutcome.Collision, -3f)]
        [TestCase(MicroArenaOutcome.Timeout, -2f)]
        [TestCase(MicroArenaOutcome.None, 0f)]
        public void TerminalReward_MatchesDocumentedConstants(MicroArenaOutcome outcome, float expected)
        {
            Assert.AreEqual(expected, MicroArenaRewardCalculator.TerminalReward(outcome));
        }
    }
}

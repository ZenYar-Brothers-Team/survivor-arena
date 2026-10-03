using Game.Content;
using UnityEngine;

namespace Game.MLAgents
{
    /// <summary>
    /// Pure reward/outcome rules for one MicroArena step, matching the formulas recorded in the research
    /// snapshot: per-step shaping toward the XP pickup, plus terminal rewards checked collect-then-collision-then-timeout.
    /// </summary>
    public static class MicroArenaRewardCalculator
    {
        public const float StepPenalty = -0.001f;
        public const float ApproachRewardScale = 0.3f;
        public const float CollectedReward = 5f;
        public const float CollisionPenalty = -3f;
        public const float TimeoutPenalty = -2f;

        /// <summary>Shaping reward for one physics step: a small time penalty plus credit for closing distance to the XP.</summary>
        public static float StepReward(float previousDistanceToXp, float currentDistanceToXp)
        {
            NumericValidation.ValidateNonNegative(previousDistanceToXp, nameof(previousDistanceToXp));
            NumericValidation.ValidateNonNegative(currentDistanceToXp, nameof(currentDistanceToXp));

            return StepPenalty + (ApproachRewardScale * (previousDistanceToXp - currentDistanceToXp));
        }

        /// <summary>Checks collect, then collision, then timeout, in that order, matching the documented experiment.</summary>
        public static MicroArenaOutcome DetermineOutcome(
            Vector2 playerPosition,
            Vector2 xpPosition,
            Vector2 threatPosition,
            MicroArenaConfig config,
            float elapsedSeconds)
        {
            if (config == null)
                throw new System.ArgumentNullException(nameof(config));
            NumericValidation.ValidateNonNegative(elapsedSeconds, nameof(elapsedSeconds));

            if (Vector2.Distance(playerPosition, xpPosition) <= config.PlayerRadius + config.XpRadius)
                return MicroArenaOutcome.Collected;

            if (Vector2.Distance(playerPosition, threatPosition) <= config.PlayerRadius + config.ThreatRadius)
                return MicroArenaOutcome.Collision;

            if (elapsedSeconds >= config.TimeLimitSeconds)
                return MicroArenaOutcome.Timeout;

            return MicroArenaOutcome.None;
        }

        public static float TerminalReward(MicroArenaOutcome outcome)
        {
            return outcome switch
            {
                MicroArenaOutcome.Collected => CollectedReward,
                MicroArenaOutcome.Collision => CollisionPenalty,
                MicroArenaOutcome.Timeout => TimeoutPenalty,
                _ => 0f
            };
        }
    }
}

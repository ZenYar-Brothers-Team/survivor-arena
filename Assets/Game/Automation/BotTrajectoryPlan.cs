using System.Collections.Generic;
using UnityEngine;

namespace Game.Automation
{
    /// <summary>Predicted geometry and risk, not a promise of damage, XP or deterministic replay.</summary>
    public sealed class BotTrajectoryPlan
    {
        public Vector2 Direction { get; }
        public Vector2? Target { get; }
        public IReadOnlyList<Vector2> Path { get; }
        public float Score { get; }
        public float PredictedXp { get; }
        public float ContactRiskSeconds { get; }
        public int EvaluatedCandidates { get; }
        public int LinearEnemyForecasts { get; }
        public double CalculationMilliseconds { get; }

        internal BotTrajectoryPlan(Vector2 direction, Vector2? target, Vector2[] path, float score,
            float predictedXp, float contactRiskSeconds, int evaluatedCandidates, int linearEnemyForecasts,
            double calculationMilliseconds)
        {
            Direction = direction;
            Target = target;
            Path = System.Array.AsReadOnly(path);
            Score = score;
            PredictedXp = predictedXp;
            ContactRiskSeconds = contactRiskSeconds;
            EvaluatedCandidates = evaluatedCandidates;
            LinearEnemyForecasts = linearEnemyForecasts;
            CalculationMilliseconds = calculationMilliseconds;
        }
    }
}

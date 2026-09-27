using Game.Content;

namespace Game.Enemy
{
    /// <summary>
    /// One more volley of the same pattern after the main shot (DECISION-0066, E1): <see cref="DelaySeconds"/> after the
    /// main volley, turned <see cref="RotationDegrees"/> from the main volley's direction (not from the current aim).
    /// </summary>
    public readonly struct EnemyAttackFollowUp
    {
        public float DelaySeconds { get; }
        public float RotationDegrees { get; }

        public EnemyAttackFollowUp(float delaySeconds, float rotationDegrees)
        {
            NumericValidation.ValidatePositive(delaySeconds, nameof(delaySeconds));
            NumericValidation.ValidateFinite(rotationDegrees, nameof(rotationDegrees));
            DelaySeconds = delaySeconds;
            RotationDegrees = rotationDegrees;
        }
    }
}

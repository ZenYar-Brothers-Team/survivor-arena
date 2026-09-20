using Game.Content;

namespace Game.Run
{
    /// <summary>Lifetime XP accounting, separate from the current level's unspent remainder.</summary>
    public sealed class RunExperienceSnapshot
    {
        public float CollectedBase { get; }
        public float CollectedAwarded { get; }
        public float ExpiredBase { get; }
        public float RecoveredAwarded { get; }
        public float InterventionBase { get; }
        public float InterventionAwarded { get; }
        public float TotalAwarded => CollectedAwarded + RecoveredAwarded + InterventionAwarded;

        public RunExperienceSnapshot(float collectedBase, float collectedAwarded, float expiredBase,
            float recoveredAwarded, float interventionBase, float interventionAwarded)
        {
            NumericValidation.ValidateNonNegative(collectedBase, nameof(collectedBase));
            NumericValidation.ValidateNonNegative(collectedAwarded, nameof(collectedAwarded));
            NumericValidation.ValidateNonNegative(expiredBase, nameof(expiredBase));
            NumericValidation.ValidateNonNegative(recoveredAwarded, nameof(recoveredAwarded));
            NumericValidation.ValidateNonNegative(interventionBase, nameof(interventionBase));
            NumericValidation.ValidateNonNegative(interventionAwarded, nameof(interventionAwarded));
            NumericValidation.ValidateNonNegative(collectedAwarded + recoveredAwarded + interventionAwarded, nameof(TotalAwarded));
            CollectedBase = collectedBase;
            CollectedAwarded = collectedAwarded;
            ExpiredBase = expiredBase;
            RecoveredAwarded = recoveredAwarded;
            InterventionBase = interventionBase;
            InterventionAwarded = interventionAwarded;
        }
    }
}

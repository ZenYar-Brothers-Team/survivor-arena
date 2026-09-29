using System;
using Game.Content;

namespace Game.Automation
{
    /// <summary>AB-13 search budget and risk preferences; no gameplay values are changed.</summary>
    public sealed class TrajectoryPolicyData
    {
        public float? HorizonSeconds { get; set; }
        public float? StepSeconds { get; set; }
        public int? CandidateCount { get; set; }
        public float? ContactPenalty { get; set; }
        public float? Clearance { get; set; }

        public void Validate()
        {
            if (!HorizonSeconds.HasValue || !StepSeconds.HasValue || !CandidateCount.HasValue ||
                !ContactPenalty.HasValue || !Clearance.HasValue)
                throw new ArgumentException("All trajectory settings are required.");
            NumericValidation.ValidateRange(HorizonSeconds.Value, 4f, 15f, nameof(HorizonSeconds));
            NumericValidation.ValidateRange(StepSeconds.Value, 0.1f, 0.5f, nameof(StepSeconds));
            NumericValidation.ValidateRange(CandidateCount.Value, 16, 256, nameof(CandidateCount));
            NumericValidation.ValidateRange(ContactPenalty.Value, 10f, 200f, nameof(ContactPenalty));
            NumericValidation.ValidateRange(Clearance.Value, 0f, 1f, nameof(Clearance));
        }
    }
}

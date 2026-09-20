using System;
using Game.Content;

namespace Game.Progression
{
    /// <summary>Definition values before/after a choice, not a live mutation or simulated cast.</summary>
    public readonly struct DraftValueChange
    {
        public string Label { get; }
        public float Current { get; }
        public float Next { get; }
        public string Unit { get; }

        public DraftValueChange(string label, float current, float next, string unit = "")
        {
            if (string.IsNullOrWhiteSpace(label)) throw new ArgumentException("Label is required.", nameof(label));
            NumericValidation.ValidateFinite(current, nameof(current));
            NumericValidation.ValidateFinite(next, nameof(next));
            Label = label;
            Current = current;
            Next = next;
            Unit = unit ?? throw new ArgumentNullException(nameof(unit));
        }
    }
}

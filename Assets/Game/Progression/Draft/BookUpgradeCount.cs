using System;
using System.Collections.Generic;
using Game.Content;

namespace Game.Progression
{
    /// <summary>
    /// DECISION-0093: a Traveler Book grants a random number of draft choices.
    /// Weight i is the relative chance of i + 1 choices; the weights need not sum to 1.
    /// </summary>
    public sealed class BookUpgradeCount
    {
        /// <summary>Neutral rule: every Book grants exactly one choice.</summary>
        public static readonly BookUpgradeCount Single = new BookUpgradeCount(new[] { 1f });

        private readonly float[] _weights;
        private readonly float _total;

        public IReadOnlyList<float> Weights => _weights;
        public int MaxCount => _weights.Length;

        public BookUpgradeCount(IReadOnlyList<float> weights)
        {
            if (weights == null) throw new ArgumentNullException(nameof(weights));
            NumericValidation.ValidateCount(weights.Count, nameof(weights));
            _weights = new float[weights.Count];
            for (var i = 0; i < weights.Count; i++)
            {
                NumericValidation.ValidateNonNegativeFinite(weights[i], nameof(weights));
                _weights[i] = weights[i];
                _total += weights[i];
            }
            NumericValidation.ValidatePositive(_total, nameof(weights));
        }

        public int Roll(IDraftRandom random)
        {
            if (random == null) throw new ArgumentNullException(nameof(random));
            if (_weights.Length == 1) return 1;
            var point = random.NextFloat01() * _total;
            for (var i = 0; i < _weights.Length; i++)
            {
                if (point < _weights[i]) return i + 1;
                point -= _weights[i];
            }
            // Float rounding at the upper edge falls back to the last non-zero weight.
            for (var i = _weights.Length - 1; i >= 0; i--)
                if (_weights[i] > 0f) return i + 1;
            return 1;
        }
    }
}

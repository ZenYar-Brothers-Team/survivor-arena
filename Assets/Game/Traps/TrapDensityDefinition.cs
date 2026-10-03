using System;
using System.Collections.Generic;
using Game.Content;
using Game.Traps.Json;

namespace Game.Traps
{
    /// <summary>
    /// Screen-based trap scatter (DECISION-0156): the arena is cut into cells of one screen; every cell gets a random trap count
    /// from the weights (mostly two, sometimes one, rarely three). The types' <c>count</c> then acts as their relative weight.
    /// </summary>
    public sealed class TrapDensityDefinition
    {
        public float CellWidth { get; }
        public float CellHeight { get; }
        public IReadOnlyList<TrapCountWeight> CountWeights { get; }
        public float TotalWeight { get; }

        public TrapDensityDefinition(TrapDensityData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            CellWidth = data.CellWidth ?? throw new ArgumentException("density.cellWidth is required.");
            CellHeight = data.CellHeight ?? throw new ArgumentException("density.cellHeight is required.");
            NumericValidation.ValidatePositive(CellWidth, nameof(CellWidth));
            NumericValidation.ValidatePositive(CellHeight, nameof(CellHeight));
            var weights = new List<TrapCountWeight>();
            var total = 0f;
            var seen = new HashSet<int>();
            foreach (var item in data.CountWeights ?? throw new ArgumentException("density.countWeights is required."))
            {
                if (item == null) throw new ArgumentException("density.countWeights cannot contain null.");
                var count = item.Count ?? throw new ArgumentException("density count is required.");
                var weight = item.Weight ?? throw new ArgumentException("density weight is required.");
                NumericValidation.ValidateNonNegative(count, "density count");
                NumericValidation.ValidatePositive(weight, "density weight");
                if (!seen.Add(count)) throw new ArgumentException($"Density count {count} is listed twice.");
                weights.Add(new TrapCountWeight(count, weight));
                total += weight;
            }
            if (weights.Count == 0) throw new ArgumentException("density.countWeights needs at least one entry.");
            CountWeights = weights.AsReadOnly();
            TotalWeight = total;
        }

        /// <summary>The trap count for a roll in [0, 1).</summary>
        public int PickCount(double roll)
        {
            var target = (float)roll * TotalWeight;
            foreach (var item in CountWeights)
            {
                if (target < item.Weight) return item.Count;
                target -= item.Weight;
            }
            return CountWeights[CountWeights.Count - 1].Count;
        }
    }
}

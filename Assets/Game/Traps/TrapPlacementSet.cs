using System.Collections.Generic;

namespace Game.Traps
{
    /// <summary>Result of laying out a field's traps for one run.</summary>
    public sealed class TrapPlacementSet
    {
        public IReadOnlyList<TrapPlacement> Traps { get; }
        public IReadOnlyList<TrapBarrelPlacement> Barrels { get; }
        /// <summary>Traps that found no valid spot within the attempt budget (0 on a healthy field).</summary>
        public int SkippedTraps { get; }
        public int SkippedBarrels { get; }

        public TrapPlacementSet(IReadOnlyList<TrapPlacement> traps, IReadOnlyList<TrapBarrelPlacement> barrels,
            int skippedTraps, int skippedBarrels)
        {
            Traps = traps;
            Barrels = barrels;
            SkippedTraps = skippedTraps;
            SkippedBarrels = skippedBarrels;
        }
    }
}

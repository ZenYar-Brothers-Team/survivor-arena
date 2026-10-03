using UnityEngine;

namespace Game.Traps
{
    /// <summary>One barrel of the run. Explosive and plain barrels are indistinguishable until the fuse is lit.</summary>
    public sealed class TrapBarrelPlacement
    {
        public Vector2 Center { get; }
        public bool Explosive { get; }
        public TrapBarrelState State { get; internal set; }
        public float FuseRemaining { get; internal set; }
        /// <summary>Seconds since the explosion; 0 before it.</summary>
        public float SecondsSinceExplosion { get; internal set; }

        public TrapBarrelPlacement(Vector2 center, bool explosive)
        {
            Center = center;
            Explosive = explosive;
        }
    }
}

using UnityEngine;

namespace Game.Traps
{
    /// <summary>A live trap projectile. Simulated by <see cref="TrapRuntime"/>; presentation only reads it.</summary>
    public sealed class TrapProjectile
    {
        public TrapProjectileDefinition Definition { get; internal set; }
        public TrapPlacement Owner { get; internal set; }
        public Vector2 Position { get; internal set; }
        /// <summary>Current flight direction (unit vector).</summary>
        public Vector2 Direction { get; internal set; }
        public float AgeSeconds { get; internal set; }
        public float DistanceTraveled { get; internal set; }
        public bool Returning { get; internal set; }
        /// <summary>Accumulated in-plane spin of round projectiles, degrees.</summary>
        public float SpinDegrees { get; internal set; }
        /// <summary>Direction of flight in degrees; directional sprites (spear, bolt) face it.</summary>
        public float FacingDegrees { get; internal set; }
    }
}

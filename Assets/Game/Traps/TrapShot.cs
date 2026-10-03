namespace Game.Traps
{
    /// <summary>One projectile of a volley, relative to the volley heading.</summary>
    public readonly struct TrapShot
    {
        public TrapProjectileDefinition Projectile { get; }
        /// <summary>Direction offset from the volley heading in degrees.</summary>
        public float AngleDegrees { get; }
        /// <summary>Sideways offset (perpendicular to the shot direction) of the muzzle, in world units.</summary>
        public float Lateral { get; }
        /// <summary>Seconds after the volley starts at which this shot leaves.</summary>
        public float DelaySeconds { get; }

        public TrapShot(TrapProjectileDefinition projectile, float angleDegrees, float lateral, float delaySeconds)
        {
            Projectile = projectile;
            AngleDegrees = angleDegrees;
            Lateral = lateral;
            DelaySeconds = delaySeconds;
        }
    }
}

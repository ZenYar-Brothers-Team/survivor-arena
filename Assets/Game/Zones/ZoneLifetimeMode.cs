namespace Game.Zones
{
    /// <summary>How long a zone exists on the field.</summary>
    public enum ZoneLifetimeMode
    {
        /// <summary>Always there, for the whole run.</summary>
        Permanent,
        /// <summary>Fades in, stays, fades out and stays gone, repeating on a fixed period; each zone has its own phase.</summary>
        Pulsing,
        /// <summary>
        /// One-shot: appears at a random point, swells for a telegraph time, goes off once, flashes away and stays gone until
        /// the next cycle, when it appears somewhere else. Only the SpeedBurst kind uses it.
        /// </summary>
        Burst
    }
}

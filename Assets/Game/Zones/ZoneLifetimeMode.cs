namespace Game.Zones
{
    /// <summary>How long a zone exists on the field.</summary>
    public enum ZoneLifetimeMode
    {
        /// <summary>Always there, for the whole run.</summary>
        Permanent,
        /// <summary>Fades in, stays, fades out and stays gone, repeating on a fixed period; each zone has its own phase.</summary>
        Pulsing
    }
}

namespace Game.Zones
{
    /// <summary>
    /// The six zone mechanics of the effect-zone foundation (magical map study). A zone is a circle on the ground; the
    /// kind decides what standing in it does.
    /// </summary>
    public enum ZoneEffectKind
    {
        /// <summary>Viscous field: slows the player and, optionally, every enemy inside.</summary>
        Slow,
        /// <summary>Wind current: speeds the player up.</summary>
        Haste,
        /// <summary>Spring: the player regenerates health per second.</summary>
        Regeneration,
        /// <summary>Arcane surge: the player's active skills hit harder and cool down faster.</summary>
        ArcanePower,
        /// <summary>Unstable rift: damage per second to the player and/or to enemies inside.</summary>
        Rift,
        /// <summary>Portal: entering teleports the player beside its paired portal (with a cooldown).</summary>
        Portal,
        /// <summary>Ward: while inside, the player takes much less damage.</summary>
        Protection,
        /// <summary>
        /// Speed burst: the disc swells to show it is about to go off, then instantly gives the player inside a timed speed
        /// buff that outlasts the zone (a Traveler-style buff).
        /// </summary>
        SpeedBurst,
        /// <summary>
        /// Charging altar: the longer the player stays inside, the more its bonus grows (to a maximum); leaving lets it drain.
        /// </summary>
        Charge
    }
}

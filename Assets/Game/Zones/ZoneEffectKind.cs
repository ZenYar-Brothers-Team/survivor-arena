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
        Charge,
        /// <summary>
        /// Strike altar: every period it shows warning circles at random points inside it, then hits everything in them once
        /// (the player, the enemies or both, by data).
        /// </summary>
        Strike,
        /// <summary>
        /// Shrine: stay inside for a while and it fires its reward once (a timed buff, healing, a blast on the enemies, a
        /// shield), then rests for a long cooldown.
        /// </summary>
        Shrine,
        /// <summary>Negative altar (mirror of Haste): enemies inside move faster.</summary>
        EnemyHaste,
        /// <summary>Negative altar (mirror of Regeneration): enemies inside regenerate health per second.</summary>
        EnemyRegeneration,
        /// <summary>Negative altar (mirror of Protection): enemies inside take much less damage.</summary>
        EnemyProtection,
        /// <summary>Negative altar (mirror of ArcanePower): enemies inside deal more damage.</summary>
        EnemyPower,
        /// <summary>Experience field: while the player stands inside, picked-up experience is multiplied.</summary>
        Experience,
        /// <summary>Knockback field: pushes every enemy inside outward from the center; the player is unaffected.</summary>
        Knockback
    }
}

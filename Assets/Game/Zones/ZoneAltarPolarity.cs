namespace Game.Zones
{
    /// <summary>
    /// Whose side an altar is on, set explicitly in data (and checked against what the effect actually does). The player
    /// reads it from the altar's marker; art decides the marker's color and shape.
    /// </summary>
    public enum ZoneAltarPolarity
    {
        /// <summary>Helps the player and never hurts them (it may hurt enemies).</summary>
        Positive,
        /// <summary>Hurts or hinders the player.</summary>
        Negative,
        /// <summary>Applies to everyone: hurts the player and the enemies alike.</summary>
        Neutral
    }
}

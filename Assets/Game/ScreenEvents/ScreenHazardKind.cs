namespace Game.ScreenEvents
{
    /// <summary>How a hazard of a screen event hurts the player (DECISION-0157).</summary>
    public enum ScreenHazardKind
    {
        /// <summary>A bar that travels along a telegraphed strip (a spear, a blade, a cloud wave).</summary>
        Sweep,
        /// <summary>An area that strikes at once and holds for a short window (a pillar, a circle, a half of the screen).</summary>
        Burst,
        /// <summary>An expanding annulus with a constant-width gap that can be crossed.</summary>
        Ring
    }
}

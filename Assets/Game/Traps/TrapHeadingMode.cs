namespace Game.Traps
{
    /// <summary>How a trap picks the base direction of a volley (DECISION-0156).</summary>
    public enum TrapHeadingMode
    {
        /// <summary>The rotation the placement was given at generation.</summary>
        Fixed,
        /// <summary>Towards the player at the moment the telegraph starts; locked until the volley is fired.</summary>
        Aim
    }
}
